using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    public class Category
    {
        public string Id { get; set; } = null!;
        public string CategoryName { get; set; } = null!;

    }
}
