using ChatApp.Areas.Identity.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ChatApp.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        public byte[] key = Convert.FromBase64String("2OF6VvT0wiAqnAgjvlHgjIEvnb+csHZHVakiEVxp8yA=");
        public byte[] iv = Convert.FromBase64String("qqp2Zv5aKGJD5YlES4VmWw==");
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

         
            var user = await _userManager.GetUserAsync(User);
            
            var __users = await _userManager.Users.ToListAsync();

            foreach (var item in __users)
            {
                await GenerateKeyPairAsync(item);

            }

            var chatContext = await _context.Messages
                .Include(m => m.Receiver)
                .Include(m => m.Sender)
                .Where(d => d.SenderId == user.Id || d.ReceiverId == user.Id)
                .ToListAsync();

            foreach (var item in chatContext)
            {
          //      item.MessageText = EncryptionHelper.DecryptMessage(item.MessageText, key, iv);
            }
            var users = await _context.AspNetUsers.ToListAsync();

            List<ChatsViewModel> chats = new List<ChatsViewModel>();


            foreach (var item in users)
            {
                chats.Add(new ChatsViewModel()
                {
                    Users = item,
                    Count = chatContext.Count(d => d.SenderId == item.Id || d.ReceiverId == item.Id),
                    Messages = chatContext.Where(d => d.SenderId == item.Id || d.ReceiverId == item.Id).ToList(),
                });
            }




            return View(chats.Where(d => d.Users.Id != user.Id).ToList());
        }

        public async Task<IActionResult> GetChats(string userId)
        {

            var user = await _userManager.GetUserAsync(User);
            var chats = await GetChatsList(userId,user);
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
            var receiverPublicKey = (await _userManager.FindByIdAsync(receiver)).SecurityStamp;
            var assUser = await _context.AspNetUsers.FirstOrDefaultAsync(d => d.Id == user.Id);
            // Encrypt the message
            byte[] encryptedMessage = EncryptionHelper.Encrypt(message, receiverPublicKey);
            var signature = EncryptionHelper.SignData(encryptedMessage, user.PhoneNumber);
            string encryptedMessageString = Convert.ToBase64String(encryptedMessage);

           // var privateKey = GetSenderPrivateKey(assUser);
            //sign message
           

            Message message1 = new Message()
            {
                SenderId = user.Id,
                ReceiverId = receiver,
                MessageText = encryptedMessageString,
                Signature = signature,
                DateSend = DateTime.Now,

            };

            _context.Add(message1);
            await _context.SaveChangesAsync();

            var chats = await GetChatsList(receiver,user);
            // chats = await _context.Messages.Where(d => d.SenderId == user.Id || d.ReceiverId == user.Id).ToListAsync();

            return View("GetChats", chats);
        }

        private async Task<List<Message>> GetChatsList(string receiver, ChatAppUser user)
        {

            var chats = await _context.Messages
                .Include(d=>d.Sender)
                .Include(d=>d.Receiver)
                .AsNoTracking().Where(d => d.SenderId == receiver || d.ReceiverId == receiver).ToListAsync();

            foreach (var item in chats)
            {

                // Receiver verifies the signature
                string receiverPublicKey = item.Sender.SecurityStamp;
                bool isSignatureValid =EncryptionHelper. VerifySignature(item.MessageText, item.Signature, receiverPublicKey);

                if (isSignatureValid)
                {
                    var key = item.SenderId != user.Id ? item.Receiver.PhoneNumber : item.Receiver.PhoneNumber;
                    var byteM = Convert.FromBase64String(item.MessageText);
                     item.MessageText = EncryptionHelper.Decrypt(byteM,key);
                    // Signature is valid, message integrity is preserved
                    // Process the decrypted message
                }
                else
                {
                    item.MessageText = "Not Valid";
                    // Signature is not valid, message may have been tampered with
                    // Handle the tampered message accordingly
                }

            }
            return chats;
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


        // Generate RSA key pair for a user
        public async Task GenerateKeyPairAsync(ChatAppUser user)
        {

         

            if (user.EmailConfirmed ==false)
            {

                using (RSA rsa = RSA.Create())
                {
                    // Generate key pair
                    RSAParameters privateKey = rsa.ExportParameters(true);
                    RSAParameters publicKey = rsa.ExportParameters(false);
               
                    // Convert keys to XML strings (for simplicity, you can choose other formats)
                    string privateKeyXml = rsa.ToXmlString(true);
                    string publicKeyXml = rsa.ToXmlString(false);

                    // Store keys in user's record
                    user.SecurityStamp = publicKeyXml;
                    user.PhoneNumber = privateKeyXml;
                    user.EmailConfirmed = true;
                    // Update user record in the database
                    await _userManager.UpdateAsync(user);
                }
            }
        }
        public RSAParameters GetSenderPrivateKey(AspNetUser user)
        {
            // Retrieve the user from the database

            // Convert public key XML string to RSAParameters
            RSAParameters senderPrivateKey;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(user.PhoneNumber);
                senderPrivateKey = rsa.ExportParameters(false); // Export public key
            }

            return senderPrivateKey;
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