using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    public class EntityQueryParameters : QueryParameters
    {
        string name;
        public string Name
        {
            get { return name; }
            set {
                if(value == null)
                    throw new NullReferenceException("You cannot specify a null name string.");
                name = value;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(name.Length);
            builder.Append("&name=");
            EncodeAndAppend(builder, name);
            return builder.ToString();
        }
    }
    
    // A person-like entity, such as an artist or a label.
    public abstract class MusicBrainzEntity : MusicBrainzObject
    {
        protected MusicBrainzEntity(string mbid)
            : base(mbid)
        {
        }

        protected MusicBrainzEntity(string mbid, string parameters)
            : base(mbid, parameters)
        {
        }

        protected MusicBrainzEntity(XmlReader reader)
            : base(reader)
        {
        }

        protected override void HandleCreateInc(StringBuilder builder)
        {
            if(aliases == null)
                AppendIncParameters(builder, "aliases");
            base.HandleCreateInc(builder);
        }

        protected void HandleLoadAllData(MusicBrainzEntity entity)
        {
            name = entity.Name;
            sort_name = entity.SortName;
            disambiguation = entity.Disambiguation;
            begin_date = entity.BeginDate;
            end_date = entity.EndDate;
            if(aliases == null)
                aliases = entity.Aliases;
            base.HandleMissingAllData(entity);
        }
        
        protected override bool HandleXml(XmlReader reader)
        {
            bool result = true;
            switch(reader.Name) {
            case "name":
				reader.Read();
				if(reader.NodeType == XmlNodeType.Text)
					name = reader.ReadContentAsString();
                break;
            case "sort-name":
				reader.Read();
				if(reader.NodeType == XmlNodeType.Text)
				   sort_name = reader.ReadContentAsString();
                break;
			case "disambiguation":
                reader.Read();
				if(reader.NodeType == XmlNodeType.Text)
					disambiguation = reader.ReadContentAsString();
                break;
            case "life-span":
                begin_date = reader["begin"];
                end_date = reader["end"];
                break;
            case "alias-list":
                if(reader.ReadToDescendant("alias")) {
                    aliases = new List<string>();
                    do {
						reader.Read();
						if(reader.NodeType == XmlNodeType.Text)
							aliases.Add(reader.ReadContentAsString());
                    } while(reader.ReadToNextSibling("alias"));
                }
                break;
            default:
				result = false;
                break;
            }
            return result;
        }

        # region Properties

        string name;
        public string Name
        {
            get {
                if(name == null)
                    LoadMissingData();
                return name;
            }
        }

        string sort_name;
        public string SortName
        {
            get {
                if(sort_name == null)
                    LoadMissingData();
                return sort_name;
            }
        }

        string disambiguation;
        public string Disambiguation
        {
            get {
                if(disambiguation == null)
                    LoadMissingData();
                return disambiguation;
            }
        }

        string begin_date;
        public string BeginDate
        {
            get {
                if(begin_date == null)
                    LoadMissingData();
                return begin_date;
            }
        }

        string end_date;
        public string EndDate
        {
            get {
                if(end_date == null)
                    LoadMissingData();
                return end_date;
            }
        }

        List<string> aliases;
        public List<string> Aliases
        {
            get {
                if(aliases == null)
                    LoadMissingData();
                return aliases ?? new List<string>();
            }
        }

        #endregion

        public override string ToString()
        {
            return name;
        }
    }
}
