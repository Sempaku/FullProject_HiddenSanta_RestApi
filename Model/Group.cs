using System.ComponentModel.DataAnnotations;

namespace Solution2.Model
{
    public class Group
    {
        public int Id { get;  set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Participant> Participants { get; set; }
        public static int globalGroupId;
        public Group()
        {
            Id = Interlocked.Increment(ref globalGroupId);
            Participants = new List<Participant>();
        }
    }
}
