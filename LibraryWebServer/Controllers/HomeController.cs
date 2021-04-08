using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryWebServer;
using LibraryWebServer.Models;

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("BasicUnitTests")]

namespace LibraryWebServer.Controllers
{
    public class HomeController : Controller
    {
        // WARNING:
        // This very simple web server is designed to be as tiny and simple as possible
        // This is NOT the way to save user data.
        // This will only allow one user of the web server at a time (aside from major security concerns).
        private static string user = "";
        private static int card = -1;

        protected Team72LibraryContext db;

        public HomeController()
        {
            db = new Team72LibraryContext();
        }

        public void UseLibraryContext(Team72LibraryContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>
        /// Given a Patron name and CardNum, verify that they exist and match in the database.
        /// If the login is successful, sets the global variables "user" and "card"
        /// </summary>
        /// <param name="name">The Patron's name</param>
        /// <param name="cardnum">The Patron's card number</param>
        /// <returns>A JSON object with a single field: "success" with a boolean value:
        /// true if the login is accepted, false otherwise.
        /// </returns>
        [HttpPost]
        public IActionResult CheckLogin(string name, int cardnum)
        {
            // TODO: Fill in. Determine if login is successful or not.
            bool loginSuccessful = false;

            //pull all patrons from table and compare name and cardnum

            var query = from p in db.Patrons select p;

            foreach (Patrons x in query)
            {
                if (x.CardNum == cardnum && x.Name == name)
                {
                    loginSuccessful = true;
                    user = name;
                    card = cardnum;
                }

            }


            if (!loginSuccessful)
            {
                return Json(new { success = false });
            }
            else
            {
                user = name;
                card = cardnum;
                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Logs a user out. This is implemented for you.
        /// </summary>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult LogOut()
        {
            user = "";
            card = -1;
            return Json(new { success = true });
        }

        /// <summary>
        /// Returns a JSON array representing all known books.
        /// Each book should contain the following fields:
        /// {"isbn" (string), "title" (string), "author" (string), "serial" (uint?), "name" (string)}
        /// Every object in the list should have isbn, title, and author.
        /// Books that are not in the Library's inventory (such as Dune) should have a null serial.
        /// The "name" field is the name of the Patron who currently has the book checked out (if any)
        /// Books that are not checked out should have an empty string "" for name.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult AllTitles()
        {

            // TODO: Implement

            //select Titles.ISBN, Title, Author, Inventory.Serial, Name from Titles left join Inventory on Titles.ISBN = Inventory.ISBN
            //left join CheckedOut on Inventory.Serial = CheckedOut.Serial left join Patrons on Patrons.CardNum = CheckedOut.CardNum;

            //Use into linq doesnt have left join 
            //use .DefaultIfEmpty() which replaces empty with null

            // Use ternary expressions for null
            // j = null ? null : j.Serial\




            var query = from t in db.Titles
                        join i in db.Inventory on t.Isbn equals i.Isbn into inv
                        from j1 in inv.DefaultIfEmpty()
                        join c in db.CheckedOut on j1.Serial equals c.Serial into chkOut
                        from j2 in chkOut.DefaultIfEmpty()
                        join p in db.Patrons on j2.CardNum equals p.CardNum into ptrn
                        from j3 in ptrn.DefaultIfEmpty()
                        select new
                        {
                            isbn = t.Isbn,
                            title = t.Title,
                            author = t.Author,
                            serial = j1 == null ? null : (uint?)j1.Serial,
                            name = j3 == null ? "" : j3.Name
                        };


            return Json(query.ToArray());



        }

        /// <summary>
        /// Returns a JSON array representing all books checked out by the logged in user 
        /// The logged in user is tracked by the global variable "card".
        /// Every object in the array should contain the following fields:
        /// {"title" (string), "author" (string), "serial" (uint) (note this is not a nullable uint) }
        /// Every object in the list should have a valid (non-null) value for each field.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult ListMyBooks()
        {
            //select Title, Author, Inventory.Serial from CheckedOut Left Join Patrons on Patrons.CardNum = CheckedOut.CardNum Left Join Inventory on Inventory.Serial = CheckedOut.Serial
            //Left Join Titles on Titles.ISBN = Inventory.ISBN where cardNum = card;

            // TODO: Implement

            var query = from c in db.CheckedOut
                        join p in db.Patrons on c.CardNum equals p.CardNum into ptrn
                        from j1 in ptrn.DefaultIfEmpty()
                        join i in db.Inventory on c.Serial equals i.Serial into inv
                        from j2 in inv.DefaultIfEmpty()
                        join t in db.Titles on j2.Isbn equals t.Isbn into ttls
                        from j3 in ttls.DefaultIfEmpty()
                        where c.CardNum == card
                        select new
                        {
                            title = j3.Title,
                            author = j3.Author,
                            serial = j2.Serial
                        };

            return Json(query.ToArray());


        }


        /// <summary>
        /// Updates the database to represent that
        /// the given book is checked out by the logged in user (global variable "card").
        /// In other words, insert a row into the CheckedOut table.
        /// You can assume that the book is not currently checked out by anyone.
        /// </summary>
        /// <param name="serial">The serial number of the book to check out</param>
        /// <returns>success</returns>
        [HttpPost]
        public ActionResult CheckOutBook(int serial)
        {
            // You may have to cast serial to a (uint)
            var query = from i in db.Inventory where i.Serial == serial select i;

            

            if(query.Count() == 0)
            {
                return Json(new { success = false });
            }


            CheckedOut newCheckout = new CheckedOut();
            newCheckout.CardNum = (uint)card;
            newCheckout.Serial = (uint)serial;


            try
            {
                db.CheckedOut.Add(newCheckout);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Save Changes for Checking out book exception: " + e);
            }

            return Json(new { success = true });

        }


        /// <summary>
        /// Returns a book currently checked out by the logged in user (global variable "card").
        /// In other words, removes a row from the CheckedOut table.
        /// You can assume the book is checked out by the user.
        /// </summary>
        /// <param name="serial">The serial number of the book to return</param>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult ReturnBook(int serial)
        {
            // You may have to cast serial to a (uint)
            // select * from CheckedOut where cardNum = cardNum and serial = serial;


            var query = from c in db.CheckedOut where c.CardNum == card && c.Serial == serial select c;


            try
            {
                db.RemoveRange(query);
                db.SaveChanges();
            }
            catch (Exception e) { Debug.WriteLine("Save Changes for returning book exception: " + e); }
        
            return Json(new { success = true });
        }

        /*******************************************/
        /****** Do not modify below this line ******/
        /*******************************************/

        /// <summary>
        /// Return the home page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }

        /// <summary>
        /// Return the MyBooks page.
        /// </summary>
        /// <returns></returns>
        public IActionResult MyBooks()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }

        /// <summary>
        /// Return the About page.
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Return the Login page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            user = "";
            card = -1;

            ViewData["Message"] = "Please login.";

            return View();
        }


        /// <summary>
        /// Return the Contact page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Return the Error page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

