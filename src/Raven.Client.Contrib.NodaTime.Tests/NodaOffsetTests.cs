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
    public class NodaOffsetTests : RavenTestBase
    {
        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Positive()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Negative()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.FromHoursAndMinutes(-5, 30));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Min()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Max()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.MaxValue);
        }

        private void Can_Use_NodaTime_Offset_In_Document(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offset = offset });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(offset, foo.Offset);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
                var expected = offset.ToTimeSpan().ToString("c");
                Assert.Equal(expected, json.Value<string>("Offset"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Positive()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index1(Offset.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Negative()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index2(Offset.FromHoursAndMinutes(-5, 30));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Min()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index1(Offset.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Max()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index2(Offset.MaxValue);
        }

        private void Can_Use_NodaTime_Offset_In_Dynamic_Index1(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offset = offset });
                    session.Store(new Foo { Id = "foos/2", Offset = offset + Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offset = offset + Offset.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset == offset);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset > offset);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset >= offset);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                }
            }
        }

        private void Can_Use_NodaTime_Offset_In_Dynamic_Index2(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offset = offset });
                    session.Store(new Foo { Id = "foos/2", Offset = offset - Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offset = offset - Offset.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset == offset);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset < offset);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset <= offset);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Positive()
        {
            Can_Use_NodaTime_Offset_In_Static_Index1(Offset.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Negative()
        {
            Can_Use_NodaTime_Offset_In_Static_Index2(Offset.FromHoursAndMinutes(-5, 30));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Min()
        {
            Can_Use_NodaTime_Offset_In_Static_Index1(Offset.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Max()
        {
            Can_Use_NodaTime_Offset_In_Static_Index2(Offset.MaxValue);
        }

        private void Can_Use_NodaTime_Offset_In_Static_Index1(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offset = offset });
                    session.Store(new Foo { Id = "foos/2", Offset = offset + Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offset = offset + Offset.FromHours(2) });
                    session.SaveChanges();
                }
                
                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset == offset);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset > offset);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset >= offset);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                }
            }
        }

        private void Can_Use_NodaTime_Offset_In_Static_Index2(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offset = offset });
                    session.Store(new Foo { Id = "foos/2", Offset = offset - Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offset = offset - Offset.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset == offset);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset < offset);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Offset <= offset);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public Offset Offset { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.Offset
                              };              
                
            }
        }
    }
}
