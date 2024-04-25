using Src.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System;
using Src.Utils;

namespace Src.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult GetList(string? keyword)
        {
            var type_id  = HttpContext.Items["type_id"];

            IQueryable<VwUsers> query = _dbContext.VwUsers;

            try{
                    
                if (type_id != null){
                    query = query.Where(z => z.type_id == Convert.ToInt32(type_id));
                }

                if (keyword != null)
                {
                    if (keyword.Length < 4)
                    {
                        return StatusCode(400, new { message = "Keyword must be 4 charachter" });
                    }

                    keyword = "%" + keyword + "%";
                    query = query.Where(
                        z =>
                            EF.Functions.Like(z.phone, keyword) ||
                            EF.Functions.Like(z.email, keyword) ||
                            EF.Functions.Like(z.work_title, keyword) ||
                            EF.Functions.Like(z.name_surname, keyword)
                    );
                }
              
                List<VwUsers> users = query.ToList();
                
                return StatusCode(200, new{count= users.Count, result= users});

            } catch (Exception ex)
            {
                Debug.WriteLine("ERR : ", ex.Message);
            }

            return StatusCode(500);
        }

        [HttpPost]
        public ActionResult Post(UsersRegisterModel register)
        {

            var sR = _dbContext.UserModel.Where(x => x.deleted_at == null && x.name_surname == register.name_surname || x.email == register.email).FirstOrDefault();

            if (sR != null)
            {
                return BadRequest(new { message = "Name Or E-mail Exist!" });
            }

            Users record = new Users();
            record.type_id = register.type_id;
            record.email = register.email;
            record.name_surname = register.name_surname;
            
            if (register.work_title != null)
                record.work_title = register.work_title;
            
            if (register.phone != null)
                record.phone = register.phone;

            try
            {
                record.created_at = DateTimeOffset.Now.ToUniversalTime();
                record.password = Util.HashPassword(register.password);
                _dbContext.UserModel.Add(record);
                _dbContext.SaveChanges();
                
                return Ok(new { id= record.id, message = "Record created !" });

            } catch (Exception ex)
            {
                Debug.WriteLine("ERR : ", ex.Message);
                return StatusCode(500);
            }
        }

        [HttpPut]
        public ActionResult Put(Users? record, string? old_password)
        {
            
            var sRu = _dbContext.UserModel.Where(x => x.deleted_at == null && x.id == record.id).FirstOrDefault();
            if (sRu == null)
            {
                return StatusCode(404, new { message = "Record detail not found"});
            }

            try
            {
                _dbContext.Entry(sRu).State = EntityState.Modified;
                sRu.updated_at = DateTime.Now.ToUniversalTime();
                
                if (record.name_surname != null && sRu.name_surname != record.name_surname)
                {
                    var sxR = _dbContext.UserModel.Where(x => x.deleted_at == null && x.id != record.id && x.name_surname == record.name_surname).FirstOrDefault();

                    if (sxR != null)
                    {
                        return BadRequest(new { message = "Name Exist!" });
                    }
                    sRu.name_surname = record.name_surname;
                }    

                if (record.email != null && sRu.email != record.email)
                {
                    var sxR = _dbContext.UserModel.Where(x => x.deleted_at == null && x.id != record.id && x.email == record.email).FirstOrDefault();

                    if (sxR != null)
                    {
                        return BadRequest(new { message = "Email Exist!" });
                    }
                    sRu.email = record.email;
                }
                
                if (record.password != null)
                {

                    if (old_password == null){
                        return BadRequest(new { message = "Old Pass must be send !" });
                    }

                    string hashedPassword = Util.HashPassword(old_password);

                    if(sRu.password == hashedPassword)
                    {
                        sRu.password = Util.HashPassword(record.password);
                    }else{
                        return BadRequest(new { message = "Password Not Match!" });
                    }

                    sRu.password = Util.HashPassword(record.password);
                }

                if (record.work_title != null && sRu.work_title != record.work_title)
                {
                    sRu.work_title = record.work_title;
                }

                if (record.phone != null && sRu.phone != record.phone)
                {
                    sRu.phone = record.phone;
                }
                
                _dbContext.SaveChanges();
                
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERR : ", e.Message);
                return StatusCode(500);
            }

            return StatusCode(200, new {message = "Updated"});
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {

            var sRu = _dbContext.UserModel.Where(x => x.deleted_at == null && x.id == id).FirstOrDefault();
            if (sRu == null)
            {
                return StatusCode(404, new { message = "Record detail not found"});
            }

            try
            {
                _dbContext.Entry(sRu).State = EntityState.Modified;
                sRu.deleted_at = DateTime.Now.ToUniversalTime();
                // or hard delete, up to u :)
                _dbContext.SaveChanges();
                
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERR : ", e.Message);
                return StatusCode(500);
            }

            return Ok(new{status = "Updated"});
        }
        
    }

}