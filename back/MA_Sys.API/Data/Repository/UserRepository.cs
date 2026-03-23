using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Users> GetByAcademia(int academiaId)
        {
            return _context.User.Where(u=> u.AcademiaId == academiaId).ToList();
        }

        public Users GetById(int id)
        {
            return _context.User.FirstOrDefault(u => u.UserId == id);
        }

        public Users GetByLogin(string login)
        {
            return _context.User.FirstOrDefault(u => u.Login == login);
        }

        public IQueryable<Users> Query(int academiaId)
        {
            return _context.User.Where(u => u.AcademiaId == academiaId);
        }
    

        public void Add(Users user)
        {
            _context.User.Add(user);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(Users user)
        {
            _context.Update(user);
        }

        public void Delete(Users user)
        {
            _context.User.Remove(user);
        }
    }
}