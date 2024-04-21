using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ChatApp.Areas.Identity.Data;

namespace ChatApp.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ChatContext _context;
        private readonly UserManager<ChatAppUser> _userManager;

        public MessagesController(ChatContext context, UserManager<ChatAppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
           
            var user  = await _userManager.GetUserAsync(User);
            var chatContext = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Sender)
                .Where(d => d.SenderId == user.Id || d.ReceiverId == user.Id)
                .ToListAsync();

            List<ChatsViewModel> chats = new List<ChatsViewModel>();

            var g1 = chatContext.Select(d => d.Sender).ToList();
            var g2 = chatContext.Select(d => d.Receiver);

             g1.AddRange(g2.ToList());

            g1 = g1.DistinctBy(d => d.Id).ToList();

            foreach (var item in g1)
            {
                chats.Add(new ChatsViewModel()
                {
                    Users = item,
                    Count = chatContext.Count(d => d.SenderId == item.Id || d.ReceiverId == item.Id),
                    Messages = chatContext.Where(d => d.SenderId == item.Id || d.ReceiverId == item.Id).ToList(),
                });
            }




            return View(chats.Where(d=>d.Users.Id != user.Id).ToList());
        }

        public async Task<IActionResult> GetChats(string userId)
        {

            var user = await _userManager.GetUserAsync(User);
            var chats = await _context.Messages.Where(d => d.SenderId == userId || d.ReceiverId == userId).ToListAsync();
           // chats = await _context.Messages.Where(d => d.SenderId == user.Id || d.ReceiverId == user.Id).ToListAsync();
            return View(chats);
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Messages == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            ViewData["SenderId"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // GET: Messages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string message, string receiver)
        {
            var user = await _userManager.GetUserAsync(User);

            Message message1 = new Message()
            {
                SenderId = user.Id,
                ReceiverId = receiver,
                MessageText = message,
                DateSend = DateTime.Now,

            };

            _context.Add(message1);
            await _context.SaveChangesAsync();

            var chats = await _context.Messages.Where(d => d.SenderId == receiver || d.ReceiverId == receiver).ToListAsync();
            // chats = await _context.Messages.Where(d => d.SenderId == user.Id || d.ReceiverId == user.Id).ToListAsync();

            return View("GetChats",chats);
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SenderId,ReceiverId,MessageText,DateSend")] Message message)
        {
            if (ModelState.IsValid)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.AspNetUsers, "Id", "Id", message.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.AspNetUsers, "Id", "Id", message.SenderId);
            return View(message);
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Messages == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            ViewData["ReceiverId"] = new SelectList(_context.AspNetUsers, "Id", "Id", message.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.AspNetUsers, "Id", "Id", message.SenderId);
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SenderId,ReceiverId,MessageText,DateSend")] Message message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.AspNetUsers, "Id", "Id", message.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.AspNetUsers, "Id", "Id", message.SenderId);
            return View(message);
        }

        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Messages == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Messages == null)
            {
                return Problem("Entity set 'ChatContext.Messages'  is null.");
            }
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
          return _context.Messages.Any(e => e.Id == id);
        }
    }
}
/* string[] randomMessages = {
            "Hello, how are you?",
            "What's up?",
            "I'm bored.",
            "Do you want to hang out?",
            "Did you see that movie?",
            "I'm hungry!",
            "Let's go for a walk.",
            "How's the weather there?",
            "Can you help me with something?",
            "I have good news!",
            "I have bad news...",
            "What do you think about this?",
            "I miss you.",
            "I'm so tired.",
            "Let's plan something fun.",
            "Have you heard the latest news?",
            "Guess what happened today?",
            "I'm excited!",
            "I'm feeling sad.",
            "Are you busy?",
            "I need your advice.",
            "Let's meet tomorrow.",
            "I'm feeling lucky!",
            "I'm feeling unlucky...",
            "This is hilarious!",
            "This is so boring...",
            "Let's go on an adventure.",
            "I have a surprise for you!",
            "I'm running late.",
            "I'll be right there."
        };
            Random random = new Random();
            var users = await _context.AspNetUsers.ToListAsync();

            foreach (var item in users)
            {
                foreach (var user2 in users)
                {
                    Message message = new Message()
                    {
                        DateSend = DateTime.Now,
                        MessageText = randomMessages[random.Next(randomMessages.Length)],
                        ReceiverId = user2.Id,
                        SenderId = item.Id,

                    };
                    _context.Add(message);
                }
            }
            await _context.SaveChangesAsync();
           */