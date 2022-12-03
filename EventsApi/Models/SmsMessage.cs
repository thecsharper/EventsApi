using System;
using System.ComponentModel.DataAnnotations;

namespace EventsApi.Models
{
    public record SmsMessage
    (
         [Required]
         Guid MessageId,
         
         [Required]
         string PhoneNumber,
         
         [Required]
         string Message
    );
}