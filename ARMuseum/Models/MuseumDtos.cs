using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Models
{
    namespace ARMuseum.Dtos
    {
        // Defines the data structure for museum data returned to the client application.
        public class MuseumDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string History { get; set; }
            public string ImageName { get; set; }
            public string? ImageUrl { get; set; } // This property is added to return the full URL of the image.
        }

        // Defines the data structure the client application will send to create or update a museum.
        public class CreateOrUpdateMuseumDto
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string History { get; set; }

            // This property is used to receive the image file from the client.
            public IFormFile? ImageFile { get; set; }
        }
    }
}