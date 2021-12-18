using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using intive_patronage.Models;

namespace intive_patronage.Controllers
{
    public class UsersController : ApiController
    {
        private intiveEntities2 db = new intiveEntities2();

        // GET: api/Users
        public IQueryable<Users> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Users/5
        [ResponseType(typeof(UserWithAddress))]
        public IHttpActionResult GetUsers(int id)
        {
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return BadRequest("User with given ID not found.");
            }

            Address A = db.Address.Find(users.AddressId);

            UserWithAddress U = new UserWithAddress();
            U.user = users;
            U.address = A;
            return Ok(U);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUsers(int id, UserWithAddress user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Users U = db.Users.Find(id);
            
            if(U == null)
            {
                return BadRequest("User with given ID not found.");
            }

            if(!U.Address.Equals(user.address))
            {
                if(U.Address.Users.Count <= 1)
                {
                    user.address.Id = U.Address.Id;
                    user.user.Address = U.Address;
                    user.user.AddressId = U.Address.Id;
                    db.Entry(U.Address).CurrentValues.SetValues(user.address);
                }
                else
                {
                    db.Address.Add(user.address);
                }
            }
            else
            {
                user.user.Address = U.Address;
                user.user.AddressId = U.AddressId;
            }
            user.user.Id = U.Id;
            db.Entry(U).CurrentValues.SetValues(user.user);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.OK);
        }

        [ResponseType(typeof(UserWithAddress))]
        // POST: api/Users
        public IHttpActionResult PostUsers(UserWithAddress U)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (U == null)
            {
                return BadRequest();
            }

            IEnumerable<Address> recycled_addresses =
                from addr in db.Address
                where addr.Country == U.address.Country && addr.City == U.address.City && addr.PostCode == U.address.PostCode && addr.HouseNumber == U.address.HouseNumber
                select addr;

            bool address_handled = false;
            if (recycled_addresses.Count() > 0)
            {
                Address recycled_address = recycled_addresses.First<Address>();
                U.user.Address = recycled_address;
                U.user.AddressId = recycled_address.Id;
                address_handled = true;
            }

            db.Users.Add(U.user);
            if (!address_handled)
            {
                db.Address.Add(U.address);
            }
            
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = U.user.Id }, U);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(Users))]
        public IHttpActionResult DeleteUsers(int id)
        {
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            Address associated_address = db.Address.Find(users.AddressId);

            if(associated_address.Users.Count <= 1)
            {
                db.Address.Remove(users.Address);
            }

            db.Users.Remove(users);
            db.SaveChanges();

            return Ok(users);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}