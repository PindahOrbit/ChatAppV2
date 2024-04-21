namespace ChatApp.Models
{
    public class ChatsViewModel
    {
        public AspNetUser Users { get; set; }
        public int Count { get; set; }
        public List<Message> Messages { get; set; }

    }
}
