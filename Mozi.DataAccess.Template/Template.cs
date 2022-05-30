namespace Mozi.DataAccess.Template
{
    public class Template
    {
        public static Template From()
        {
            Template tp = new Template();
            return tp;
        }
    }
    /// <summary>
    /// DDL谓词
    /// </summary>
    public class DDLObject
    {

    }

    public class Create : DDLObject
    {

    }

    public class Drop : DDLObject
    {

    }

    public class Alter : DDLObject
    {

    }

    public class Truncate : DDLObject
    {

    }

    public class Comment : DDLObject
    {

    }

    public class Rename : DDLObject
    {

    }

    public class DMLObject
    {

    }

    public class SelectObject : DMLObject
    {

    }

    public class UpdateObject : DMLObject
    {

    }

    public class InsertObject : DMLObject
    {

    }

    public class DeleteObject : DMLObject
    {

    }

    public static class Extensions
    {
        public static void Where(this DMLObject dm, string whereargs)
        {

        }

        public static DMLObject Select(this Template data, string whereargs)
        {
            return new DMLObject();
        }

        public static void Update(this Template data, string whereargs)
        {

        }
        public static void Insert(this Template data, string whereargs)
        {

        }
        public static void Delete(this Template data, string whereargs)
        {

        }
    }
}
