using generala.Models.Database;
using generala.Models.Database.Entities;
using generala.Models.Database.Repositories;
namespace generala.Models.Database.Repositories.Implementations;

public class ImageRepository : Repository<Image, int>
{
    public ImageRepository(GeneralaContext context) : base(context)
    {
        
    }
}
