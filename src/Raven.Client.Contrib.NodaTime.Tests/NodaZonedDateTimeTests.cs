﻿using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Client.Indexes;
using Raven.Client.NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.Contrib.NodaTime.Tests
{
    public class NodaZonedDateTimeTests : RavenTestBase
    {
		// NOTE: Tests are intentionally omited for very early dates.
		//       This is because most of the timezones did not exist then, so their values
		//       are meaningless.  ZonedDateTime is only for values that are actually
		//       valid at some point in the time zone's history.

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Document_Now()
        {
			var instant = SystemClock.Instance.Now;
	        var zone = DateTimeZoneProviders.Default.GetSystemDefault();
	        var zdt = new ZonedDateTime(instant, zone);
	        Can_Use_NodaTime_ZonedDateTime_In_Document(zdt);
        }

		[Fact]
		public void Can_Use_NodaTime_ZonedDateTime_In_Document_NearIsoMax()
        {
			var instant = NodaUtil.Instant.MaxIsoValue - Duration.FromHours(24);
			var zone = DateTimeZoneProviders.Default.GetSystemDefault();
			var zdt = new ZonedDateTime(instant, zone);
			Can_Use_NodaTime_ZonedDateTime_In_Document(zdt);
        }

        private void Can_Use_NodaTime_ZonedDateTime_In_Document(ZonedDateTime zdt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", ZonedDateTime = zdt });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(zdt, foo.ZonedDateTime);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
				
	            var expectedDateTime = zdt.ToDateTimeOffset().ToString("o");
	            var expectedZone = zdt.Zone.Id;
				Assert.Equal(expectedDateTime, json["ZonedDateTime"].Value<string>("DateTime"));
	            Assert.Equal(expectedZone, json["ZonedDateTime"].Value<string>("Zone"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index_Now()
        {
			var instant = SystemClock.Instance.Now;
			var zone = DateTimeZoneProviders.Default.GetSystemDefault();
			Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index(new ZonedDateTime(instant, zone));
        }

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index_NearIsoMax()
        {
			var instant = NodaUtil.Instant.MaxIsoValue - Duration.FromHours(24);
			var zone = DateTimeZoneProviders.Default.GetSystemDefault();
			Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index(new ZonedDateTime(instant, zone));
        }

        private void Can_Use_NodaTime_ZonedDateTime_In_Dynamic_Index(ZonedDateTime zdt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", ZonedDateTime = zdt });
					session.Store(new Foo { Id = "foos/2", ZonedDateTime = zdt - Duration.FromMinutes(1) });
					session.Store(new Foo { Id = "foos/3", ZonedDateTime = zdt - Duration.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.ZonedDateTime == zdt);
					Debug.WriteLine(q1);
					var results1 = q1.ToList();
                    WaitForUserToContinueTheTest(documentStore);
					Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.ZonedDateTime.ToInstant() < zdt.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

					var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.ZonedDateTime.ToInstant() <= zdt.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Static_Index_Now()
        {
			var instant = SystemClock.Instance.Now;
			var zone = DateTimeZoneProviders.Default.GetSystemDefault();
			Can_Use_NodaTime_ZonedDateTime_In_Static_Index(new ZonedDateTime(instant, zone));
        }

        [Fact]
        public void Can_Use_NodaTime_ZonedDateTime_In_Static_Index_NearIsoMax()
        {
			var instant = NodaUtil.Instant.MaxIsoValue - Duration.FromHours(24);
			var zone = DateTimeZoneProviders.Default.GetSystemDefault();
			Can_Use_NodaTime_ZonedDateTime_In_Static_Index(new ZonedDateTime(instant, zone));
        }

        private void Can_Use_NodaTime_ZonedDateTime_In_Static_Index(ZonedDateTime zdt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", ZonedDateTime = zdt });
					session.Store(new Foo { Id = "foos/2", ZonedDateTime = zdt - Duration.FromMinutes(1) });
					session.Store(new Foo { Id = "foos/3", ZonedDateTime = zdt - Duration.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.ZonedDateTime == zdt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

					var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.ZonedDateTime.ToInstant() < zdt.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

					var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.ZonedDateTime.ToInstant() <= zdt.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public ZonedDateTime ZonedDateTime { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.ZonedDateTime
                              };
            }
        }
    }
}
