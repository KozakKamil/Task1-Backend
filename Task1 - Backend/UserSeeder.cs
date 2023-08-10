using Task1Backend.Entities;

namespace Task1Backend
{
    public class UserSeeder
    {
        private readonly DataContext _context;

        public UserSeeder(DataContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            if (_context.Database.CanConnect())
            {
                if (!_context.Roles.Any())
                {
                    var roles = GetRoles();
                    _context.Roles.AddRange(roles);
                    _context.SaveChanges();
                }
            }
        }

        private IEnumerable<Role> GetRoles()
        {
            var roles = new List<Role>()
            {
                new Role()
                {
                    Name = "Admin"
                },
                new Role()
                {
                    Name = "User"
                },
                new Role()
                {
                    Name = "Guest"
                }
            };
            return roles;
        }
    }
}