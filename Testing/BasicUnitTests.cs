using System;
using Xunit;
using LibraryWebServer.Controllers;
using LibraryWebServer.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Testing
{
    public class BasicUnitTests
    {

        [Fact]
        public void Test1()
        {
            HomeController c = new HomeController();

            c.UseLibraryContext(MakeTinyDB());

            var allTitlesResult = c.AllTitles() as JsonResult;

            dynamic x = allTitlesResult.Value;

            Assert.Equal(3, x.Length);
        }

        [Fact]
        public void Test2()
        {
            HomeController c = new HomeController();

            Team72LibraryContext db = MakeDB();

            c.UseLibraryContext(db);

            c.CheckLogin("fake", 4);

            c.CheckOutBook(1);

            var query = from co in db.CheckedOut select co;
            
            Assert.Equal(1, query.Count());

            Assert.Equal((uint)4, query.ToArray()[0].CardNum);

        }

        [Fact]
        public void ReturnBookNotThere()
        {
            HomeController c = new HomeController();

            Team72LibraryContext db = MakeDB();

            c.UseLibraryContext(db);

            c.CheckLogin("fake", 4);

            c.ReturnBook(2);

            var query = from co in db.CheckedOut select co;

            Assert.Equal(0, query.Count());

        }

        [Fact]
        public void CheckoutAlreadyCheckedOutBook()
        {
            HomeController c = new HomeController();

            Team72LibraryContext db = MakeTinyDB();

            c.UseLibraryContext(db);

            c.CheckLogin("fake2", 2);

            c.CheckOutBook(3);

            var query = from co in db.CheckedOut select co;

            Assert.Equal(1, query.Count());

        }

        [Fact]
        public void LoginLogout()
        {
            HomeController c = new HomeController();

            Team72LibraryContext db = MakeDB();

            c.UseLibraryContext(db);

            c.CheckLogin("fake2", 2);

            c.CheckOutBook(3);

            c.LogOut();

            c.CheckLogin("fake", 4);

            c.CheckOutBook(1);
            c.CheckOutBook(2);

            var query = from co in db.CheckedOut select co;

            Assert.Equal(3, query.Count());

        }

        [Fact]
        public void LoginLogoutplusReturn()
        {
            HomeController c = new HomeController();

            Team72LibraryContext db = MakeDB();

            c.UseLibraryContext(db);

            c.CheckLogin("fake2", 2);

            c.CheckOutBook(3);

            c.LogOut();

            c.CheckLogin("fake", 4);

            c.CheckOutBook(1);
            c.CheckOutBook(2);

            c.LogOut();

            c.CheckLogin("fake2", 2);

            c.ReturnBook(3);

            c.LogOut();

            c.CheckLogin("fake", 4);

            c.ReturnBook(2);
            c.ReturnBook(1);

            var query = from co in db.CheckedOut select co;

            Assert.Equal(0, query.Count());

        }

        [Fact]
        public void FakeBookCheckout()
        {
            HomeController c = new HomeController();

            Team72LibraryContext db = MakeDB();

            c.UseLibraryContext(db);

            c.CheckLogin("fake2", 2);

            c.CheckOutBook(1001);

            var query = from co in db.CheckedOut select co;

            Assert.Equal(0, query.Count());


        }

        [Fact]
        public void FakeBookreturn()
        {
            HomeController c = new HomeController();

            Team72LibraryContext db = MakeDB();

            c.UseLibraryContext(db);

            c.CheckLogin("fake2", 2);

            c.CheckOutBook(1001);

            var query = from co in db.CheckedOut select co;

            Assert.Equal(0, query.Count());

        }








        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
              .AddEntityFrameworkInMemoryDatabase()
              .BuildServiceProvider();
            return serviceProvider;
        }

        Team72LibraryContext MakeTinyDB()
        {
            var optionsBuilder = new DbContextOptionsBuilder<Team72LibraryContext>();
            optionsBuilder.UseInMemoryDatabase("medium_library").UseApplicationServiceProvider(NewServiceProvider());

            Team72LibraryContext db = new Team72LibraryContext(optionsBuilder.Options);

            Titles t = new Titles();
            t.Author = "author1";
            t.Title = "title1";
            t.Isbn = "123-1231231231";

            Titles t2 = new Titles();
            t2.Author = "author2";
            t2.Title = "title2";
            t2.Isbn = "123-1231231232";

            Titles t3 = new Titles();
            t3.Author = "author3";
            t3.Title = "title3";
            t3.Isbn = "123-1231231233";

            Inventory i = new Inventory();
            i.Isbn = "123-1231231231";
            i.Serial = 1;

            Inventory i2 = new Inventory();
            i2.Isbn = "123-1231231232";
            i2.Serial = 2;

            Inventory i3 = new Inventory();
            i3.Isbn = "123-1231231233";
            i3.Serial = 3;

            Patrons p = new Patrons();
            p.CardNum = 1;
            p.Name = "fake";

            Patrons p2 = new Patrons();
            p2.CardNum = 2;
            p2.Name = "fake2";

            CheckedOut c = new CheckedOut();
            c.CardNum = 1;
            c.Serial = 3;

            db.Titles.Add(t);
            db.Titles.Add(t2);
            db.Titles.Add(t3);
            db.Inventory.Add(i);
            db.Inventory.Add(i2);
            db.Inventory.Add(i3);
            db.Patrons.Add(p);
            db.Patrons.Add(p2);
            db.CheckedOut.Add(c);
            db.SaveChanges();

            return db;
        }

        Team72LibraryContext MakeDB()
        {
            var optionsBuilder = new DbContextOptionsBuilder<Team72LibraryContext>();
            optionsBuilder.UseInMemoryDatabase("medium_library").UseApplicationServiceProvider(NewServiceProvider());

            Team72LibraryContext db = new Team72LibraryContext(optionsBuilder.Options);

            Titles t = new Titles();
            t.Author = "author1";
            t.Title = "title1";
            t.Isbn = "123-1231231231";

            Titles t2 = new Titles();
            t2.Author = "author2";
            t2.Title = "title2";
            t2.Isbn = "123-1231231232";

            Titles t3 = new Titles();
            t3.Author = "author3";
            t3.Title = "title3";
            t3.Isbn = "123-1231231233";

            Inventory i = new Inventory();
            i.Isbn = "123-1231231231";
            i.Serial = 1;

            Inventory i2 = new Inventory();
            i2.Isbn = "123-1231231232";
            i2.Serial = 2;

            Inventory i3 = new Inventory();
            i3.Isbn = "123-1231231233";
            i3.Serial = 3;

            Patrons p = new Patrons();
            p.Name = "fake";
            p.CardNum = 4;
            db.Patrons.Add(p);

            Patrons p2 = new Patrons();
            p2.CardNum = 2;
            p2.Name = "fake2";


            db.Titles.Add(t);
            db.Titles.Add(t2);
            db.Titles.Add(t3);
            db.Inventory.Add(i);
            db.Inventory.Add(i2);
            db.Inventory.Add(i3);
            
            db.Patrons.Add(p2);
            db.SaveChanges();

            return db;
        }
    }

}
