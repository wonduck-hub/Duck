
namespace DuckChatForm.Models
{
    public class User
    {
        public string Name { get; private set; }
        public Guid ID { get; private set; }

        public User(string name)
        {
            Name = name;
            ID = Guid.NewGuid();
        }
    }
}
