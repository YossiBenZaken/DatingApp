using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DattingApp.API.Helpers;
using DattingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DattingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userID, int recipientID)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerID == userID && u.LikeeID == recipientID);
        }

        public async Task<Photo> GetMainPhotoForUser(int userID)
        {
            return await _context.Photos.Where(u => u.UserID == userID).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ID == id);
            return photo;
        }

        public async Task<User> GetUser(int id, bool isCurrentUser)
        {
            var query = _context.Users.Include(p => p.Photos).AsQueryable();
            if(isCurrentUser)
                query = query.IgnoreQueryFilters();
            var user = await query.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserID);
            users = users.Where(u => u.Gender == userParams.Gender);
            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserID, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserID, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.Include(x => x.Likers).Include(x => x.Likees).FirstOrDefaultAsync(u => u.Id == id);
            if(likers) {
                return user.Likers.Where(u => u.LikeeID == id).Select(i => i.LikerID);
            }
            else{
                return user.Likees.Where(u => u.LikerID == id).Select(i => i.LikeeID);
            }
        }
        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.ID == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos).Include(u => u.Recipient).ThenInclude(p => p.Photos).AsQueryable();
            switch(messageParams.MessageContainer){
                case "Inbox":
                    messages = messages.Where(u => u.RecipientID == messageParams.UserID && u.RecipientDelete == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderID == messageParams.UserID && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientID == messageParams.UserID && u.SenderDeleted == false && u.IsRead == false);
                    break;
            }
            messages = messages.OrderByDescending(d => d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages,messageParams.PageNumber,messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userID, int recipientID)
        {
            var messages = await _context.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => m.RecipientID == userID && m.RecipientDelete == false && m.SenderID == recipientID || m.RecipientID == recipientID && m.SenderID == userID && m.SenderDeleted == false)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();
            return messages;
        }
    }
}