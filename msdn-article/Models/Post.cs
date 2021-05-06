using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolrNet.Attributes;

namespace MSDNArticleDemo.Models
{
    /// <summary>
    /// This class represents a document in the search engine
    /// </summary>
    class Post
    {
        /// <summary>
        /// Gets or sets the document id
        /// <field  name="id" type="string" indexed="true" stored="true" required="true" multiValued="false" />
        /// </summary>
        [SolrUniqueKey("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets postTypeId
        /// <field name="postTypeId" type="pint" indexed="true" stored="true" />
        /// </summary>
        [SolrField("postTypeId")]
        public int PostTypeId { get; set; }

        /// <summary>
        /// Gets or sets title
        /// <field name="title" type="text_gen_sort" indexed="true" stored="true" />
        /// </summary>
        [SolrField("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets body
        /// <field name="body" type="text_general" indexed="false" stored="true" />
        /// </summary>
        [SolrField("body")]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets tags
        /// <field name="tags" type="string" indexed="true" stored="true" multiValued="true"/>
        /// </summary>
        [SolrField("tags")]
        public ICollection<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets postScore
        /// <field name="postScore" type="pfloat" indexed="true" stored="true"/>
        /// </summary>
        [SolrField("postScore")]
        public float PostScore { get; set; }

        /// <summary>
        /// Gets or sets ownerUserId
        /// <field name="ownerUserId" type="pint" indexed="true" stored="true" />
        /// </summary>
        [SolrField("ownerUserId")]
        public int? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets answerCount
        /// <field name="answerCount" type="pint" indexed="true" stored="true" />
        /// </summary>
        [SolrField("answerCount")]
        public int? AnswerCount { get; set; }

        /// <summary>
        /// Gets or sets commentCount
        /// <field name="commentCount" type="pint" indexed="true" stored="true" />
        /// </summary>
        [SolrField("commentCount")]
        public int CommentCount { get; set; }

        /// <summary>
        /// Gets or sets favoriteCount
        /// <field name="favoriteCount" type="pint" indexed="true" stored="true" />
        /// </summary>
        [SolrField("favoriteCount")]
        public int? FavoriteCount { get; set; }

        /// <summary>
        /// Gets or sets viewCount
        /// <field name="viewCount" type="pint" indexed="true" stored="true" />
        /// </summary>
        [SolrField("viewCount")]
        public int? ViewCount { get; set; }

        /// <summary>
        /// Gets or sets creationDate
        /// <field name="creationDate" type="pdate" indexed="true" stored="true" />
        /// </summary>
        [SolrField("creationDate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets lastActivityDate
        /// <field name="lastActivityDate" type="pdate" indexed="true" stored="true" />
        /// </summary>
        [SolrField("lastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        /// <summary>
        /// Gets or sets closedDate
        /// <field name="closedDate" type="pdate" indexed="true" stored="true" />
        /// </summary>
        [SolrField("closedDate")]
        public DateTime? ClosedDate { get; set; }

    }
}
