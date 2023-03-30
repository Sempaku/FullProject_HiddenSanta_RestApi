namespace Solution2.Model
{
    public class Participant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Wish { get; set; }
        public Participant Recipient { get; set; }
        public static int globalParticipantId;
        public Participant()
        {
            Id = Interlocked.Increment(ref globalParticipantId);
        }
    }
}
