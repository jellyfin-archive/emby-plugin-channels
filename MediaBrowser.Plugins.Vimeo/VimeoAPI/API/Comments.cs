using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MediaBrowser.Plugins.Vimeo.VimeoAPI.API
{
    public class Comments : List<Comment>
    {
        public int on_this_page;
        public int page;
        public int perpage;
        public int total;

        public static Comments FromElement(XElement e)
        {
            var cs = new Comments
            {
                on_this_page = int.Parse(e.Attribute("on_this_page").Value),
                page = int.Parse(e.Attribute("page").Value),
                perpage = int.Parse(e.Attribute("perpage").Value),
                total = int.Parse(e.Attribute("total").Value)
            };
            cs.AddRange(e.Elements("comment").Select(Comment.FromElement));
            return cs;
        }
    }

    public class Comment
    {
        public string datecreate;
        public string id;
        public string permalink;
        public string reply_to_comment_id;
        public string type;
        public string text;
        public Contact author;

        public static Comment FromElement(XElement e)
        {
            return new Comment
            {
                datecreate = e.Attribute("datecreate").Value,
                id = e.Attribute("id").Value,
                permalink = e.Attribute("permalink").Value,
                reply_to_comment_id = e.Attribute("reply_to_comment_id").Value,
                type = e.Attribute("type").Value,
                text = e.Element("text").Value,
                author = Contact.FromElement(e.Element("author"))
            };
        }
    }
}
