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
	// TODO: Periods are tricky.  We should probably normalize them and allow for equivalency when querying.

	public class NodaPeriodTests : RavenTestBase
	{
		[Fact]
		public void Can_Use_NodaTime_Period_In_Document_Positive()
		{
			Can_Use_NodaTime_Period_In_Document(Period.FromHours(2));
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Document_Negative()
		{
			Can_Use_NodaTime_Period_In_Document(Period.FromHours(-5));
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Document_Min()
		{
			Can_Use_NodaTime_Period_In_Document(NodaUtil.Period.MinValue);
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Document_Max()
		{
			Can_Use_NodaTime_Period_In_Document(NodaUtil.Period.MaxValue);
		}

		private void Can_Use_NodaTime_Period_In_Document(Period period)
		{
			using (var documentStore = NewDocumentStore())
			{
				documentStore.ConfigureForNodaTime();

				using (var session = documentStore.OpenSession())
				{
					session.Store(new Foo { Id = "foos/1", Period = period });
					session.SaveChanges();
				}

				using (var session = documentStore.OpenSession())
				{
					var foo = session.Load<Foo>("foos/1");

					Assert.Equal(period, foo.Period);
				}

				var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
				Debug.WriteLine(json.ToString(Formatting.Indented));
				var expected = period.ToString();
				Assert.Equal(expected, json.Value<string>("Period"));
			}
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Dynamic_Index_Positive()
		{
			Can_Use_NodaTime_Period_In_Dynamic_Index1(Period.FromHours(2));
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Dynamic_Index_Negative()
		{
			Can_Use_NodaTime_Period_In_Dynamic_Index2(Period.FromHours(-5));
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Dynamic_Index_Min()
		{
			Can_Use_NodaTime_Period_In_Dynamic_Index1(NodaUtil.Period.MinValue);
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Dynamic_Index_Max()
		{
			Can_Use_NodaTime_Period_In_Dynamic_Index2(NodaUtil.Period.MaxValue);
		}

		private void Can_Use_NodaTime_Period_In_Dynamic_Index1(Period period)
		{
			using (var documentStore = NewDocumentStore())
			{
				documentStore.ConfigureForNodaTime();

				using (var session = documentStore.OpenSession())
				{
					session.Store(new Foo { Id = "foos/1", Period = period });
					session.Store(new Foo { Id = "foos/2", Period = period + Period.FromHours(1) });
					session.Store(new Foo { Id = "foos/3", Period = period + Period.FromHours(2) });
					session.SaveChanges();
				}

				using (var session = documentStore.OpenSession())
				{
					var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).OrderBy(x => x.Period).Where(x => x.Period == period);
					var results1 = q1.ToList();
					Assert.Equal(1, results1.Count);

					// Period does not implement IComparable, so you can't query with greater then or less than
				}
			}
		}

		private void Can_Use_NodaTime_Period_In_Dynamic_Index2(Period period)
		{
			using (var documentStore = NewDocumentStore())
			{
				documentStore.ConfigureForNodaTime();

				using (var session = documentStore.OpenSession())
				{
					session.Store(new Foo { Id = "foos/1", Period = period });
					session.Store(new Foo { Id = "foos/2", Period = period - Period.FromHours(1) });
					session.Store(new Foo { Id = "foos/3", Period = period - Period.FromHours(2) });
					session.SaveChanges();
				}

				using (var session = documentStore.OpenSession())
				{
					var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Period == period);
					var results1 = q1.ToList();
					Assert.Equal(1, results1.Count);

					// Period does not implement IComparable, so you can't query with greater then or less than
				}
			}
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Static_Index_Positive()
		{
			Can_Use_NodaTime_Period_In_Static_Index1(Period.FromHours(2));
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Static_Index_Negative()
		{
			Can_Use_NodaTime_Period_In_Static_Index2(Period.FromHours(-5));
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Static_Index_Min()
		{
			Can_Use_NodaTime_Period_In_Static_Index1(NodaUtil.Period.MinValue);
		}

		[Fact]
		public void Can_Use_NodaTime_Period_In_Static_Index_Max()
		{
			Can_Use_NodaTime_Period_In_Static_Index2(NodaUtil.Period.MaxValue);
		}

		private void Can_Use_NodaTime_Period_In_Static_Index1(Period period)
		{
			using (var documentStore = NewDocumentStore())
			{
				documentStore.ConfigureForNodaTime();
				documentStore.ExecuteIndex(new TestIndex());

				using (var session = documentStore.OpenSession())
				{
					session.Store(new Foo { Id = "foos/1", Period = period });
					session.Store(new Foo { Id = "foos/2", Period = period + Period.FromHours(1) });
					session.Store(new Foo { Id = "foos/3", Period = period + Period.FromHours(2) });
					session.SaveChanges();
				}

				using (var session = documentStore.OpenSession())
				{
					var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Period == period);
					var results1 = q1.ToList();
					Assert.Equal(1, results1.Count);

					// Period does not implement IComparable, so you can't query with greater then or less than
				}
			}
		}

		private void Can_Use_NodaTime_Period_In_Static_Index2(Period period)
		{
			using (var documentStore = NewDocumentStore())
			{
				documentStore.ConfigureForNodaTime();
				documentStore.ExecuteIndex(new TestIndex());

				using (var session = documentStore.OpenSession())
				{
					session.Store(new Foo { Id = "foos/1", Period = period });
					session.Store(new Foo { Id = "foos/2", Period = period - Period.FromHours(1) });
					session.Store(new Foo { Id = "foos/3", Period = period - Period.FromHours(2) });
					session.SaveChanges();
				}

				using (var session = documentStore.OpenSession())
				{
					var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Period == period);
					var results1 = q1.ToList();
					Assert.Equal(1, results1.Count);

					// Period does not implement IComparable, so you can't query with greater then or less than
				}
			}
		}

		public class Foo
		{
			public string Id { get; set; }
			public Period Period { get; set; }
		}

		public class TestIndex : AbstractIndexCreationTask<Foo>
		{
			public TestIndex()
			{
				Map = foos => from foo in foos
				              select new
				                     {
					                     foo.Period
				                     };
			}
		}
	}
}
