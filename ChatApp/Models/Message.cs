using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Models
{
    public partial class Message
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        [Column("Sender_ID")]
        [StringLength(450)]
        public string SenderId { get; set; }
        [Required]
        [Column("Receiver_ID")]
        [StringLength(450)]
        public string ReceiverId { get; set; }
        [Required]
        [Column("Message_Text")]
        public string MessageText { get; set; }
        [Column("date_send", TypeName = "datetime")]
        public DateTime DateSend { get; set; }
        [Required]
        [Column("signature")]
        public byte[] Signature { get; set; }

        [ForeignKey("ReceiverId")]
        [InverseProperty("MessageReceivers")]
        public virtual AspNetUser Receiver { get; set; }
        [ForeignKey("SenderId")]
        [InverseProperty("MessageSenders")]
        public virtual AspNetUser Sender { get; set; }
    }
}
