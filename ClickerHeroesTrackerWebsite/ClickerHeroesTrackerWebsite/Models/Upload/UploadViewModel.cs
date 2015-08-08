namespace ClickerHeroesTrackerWebsite.Models.Upload
{
    using System.ComponentModel.DataAnnotations;

    public class UploadViewModel
    {
        [Required]
        [Display(Name = "Save Data")]
        public string EncodedSaveData { get; set; }

        [Display(Name = "Add this upload to my progress")]
        public bool AddToProgress { get; set; }
    }
}