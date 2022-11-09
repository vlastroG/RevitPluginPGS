/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace Xml2CSharp
{
    [XmlRoot(ElementName = "linkage")]
    public class Linkage
    {
        [XmlAttribute(AttributeName = "mode")]
        public string Mode { get; set; }
    }

    [XmlRoot(ElementName = "clashselection")]
    public class Clashselection
    {
        [XmlElement(ElementName = "locator")]
        public string Locator { get; set; }
        [XmlAttribute(AttributeName = "selfintersect")]
        public string Selfintersect { get; set; }
        [XmlAttribute(AttributeName = "primtypes")]
        public string Primtypes { get; set; }
    }

    [XmlRoot(ElementName = "left")]
    public class Left
    {
        [XmlElement(ElementName = "clashselection")]
        public Clashselection Clashselection { get; set; }
    }

    [XmlRoot(ElementName = "right")]
    public class Right
    {
        [XmlElement(ElementName = "clashselection")]
        public Clashselection Clashselection { get; set; }
    }

    [XmlRoot(ElementName = "summary")]
    public class Summary
    {
        [XmlElement(ElementName = "testtype")]
        public string Testtype { get; set; }
        [XmlElement(ElementName = "teststatus")]
        public string Teststatus { get; set; }
        [XmlAttribute(AttributeName = "total")]
        public string Total { get; set; }
        [XmlAttribute(AttributeName = "new")]
        public string New { get; set; }
        [XmlAttribute(AttributeName = "active")]
        public string Active { get; set; }
        [XmlAttribute(AttributeName = "reviewed")]
        public string Reviewed { get; set; }
        [XmlAttribute(AttributeName = "approved")]
        public string Approved { get; set; }
        [XmlAttribute(AttributeName = "resolved")]
        public string Resolved { get; set; }
    }

    [XmlRoot(ElementName = "pos3f")]
    public class Pos3f
    {
        [XmlAttribute(AttributeName = "x")]
        public string X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public string Y { get; set; }
        [XmlAttribute(AttributeName = "z")]
        public string Z { get; set; }
    }

    [XmlRoot(ElementName = "clashpoint")]
    public class Clashpoint
    {
        [XmlElement(ElementName = "pos3f")]
        public Pos3f Pos3f { get; set; }
    }

    [XmlRoot(ElementName = "date")]
    public class Date
    {
        [XmlAttribute(AttributeName = "year")]
        public string Year { get; set; }
        [XmlAttribute(AttributeName = "month")]
        public string Month { get; set; }
        [XmlAttribute(AttributeName = "day")]
        public string Day { get; set; }
        [XmlAttribute(AttributeName = "hour")]
        public string Hour { get; set; }
        [XmlAttribute(AttributeName = "minute")]
        public string Minute { get; set; }
        [XmlAttribute(AttributeName = "second")]
        public string Second { get; set; }
    }

    [XmlRoot(ElementName = "createddate")]
    public class Createddate
    {
        [XmlElement(ElementName = "date")]
        public Date Date { get; set; }
    }

    [XmlRoot(ElementName = "objectattribute")]
    public class Objectattribute
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "smarttag")]
    public class Smarttag
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "smarttags")]
    public class Smarttags
    {
        [XmlElement(ElementName = "smarttag")]
        public List<Smarttag> Smarttag { get; set; }
    }

    [XmlRoot(ElementName = "clashobject")]
    public class Clashobject
    {
        [XmlElement(ElementName = "objectattribute")]
        public Objectattribute Objectattribute { get; set; }
        [XmlElement(ElementName = "layer")]
        public string Layer { get; set; }
        [XmlElement(ElementName = "smarttags")]
        public Smarttags Smarttags { get; set; }
    }

    [XmlRoot(ElementName = "clashobjects")]
    public class Clashobjects
    {
        [XmlElement(ElementName = "clashobject")]
        public List<Clashobject> Clashobject { get; set; }
    }

    [XmlRoot(ElementName = "comment")]
    public class Comment
    {
        [XmlElement(ElementName = "user")]
        public string User { get; set; }
        [XmlElement(ElementName = "body")]
        public string Body { get; set; }
        [XmlElement(ElementName = "createddate")]
        public Createddate Createddate { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
    }

    [XmlRoot(ElementName = "comments")]
    public class Comments
    {
        [XmlElement(ElementName = "comment")]
        public List<Comment> Comment { get; set; }
    }

    [XmlRoot(ElementName = "clashresult")]
    public class Clashresult
    {
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "resultstatus")]
        public string Resultstatus { get; set; }
        [XmlElement(ElementName = "clashpoint")]
        public Clashpoint Clashpoint { get; set; }
        [XmlElement(ElementName = "gridlocation")]
        public string Gridlocation { get; set; }
        [XmlElement(ElementName = "createddate")]
        public Createddate Createddate { get; set; }
        [XmlElement(ElementName = "assignedto")]
        public string Assignedto { get; set; }
        [XmlElement(ElementName = "clashobjects")]
        public Clashobjects Clashobjects { get; set; }
        [XmlElement(ElementName = "comments")]
        public Comments Comments { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "guid")]
        public string Guid { get; set; }
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "distance")]
        public string Distance { get; set; }
    }

    [XmlRoot(ElementName = "clashresults")]
    public class Clashresults
    {
        [XmlElement(ElementName = "clashresult")]
        public List<Clashresult> Clashresult { get; set; }
    }

    [XmlRoot(ElementName = "clashtest")]
    public class Clashtest
    {
        [XmlElement(ElementName = "linkage")]
        public Linkage Linkage { get; set; }
        [XmlElement(ElementName = "left")]
        public Left Left { get; set; }
        [XmlElement(ElementName = "right")]
        public Right Right { get; set; }
        [XmlElement(ElementName = "rules")]
        public string Rules { get; set; }
        [XmlElement(ElementName = "summary")]
        public Summary Summary { get; set; }
        [XmlElement(ElementName = "clashresults")]
        public Clashresults Clashresults { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "test_type")]
        public string Test_type { get; set; }
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }
        [XmlAttribute(AttributeName = "tolerance")]
        public string Tolerance { get; set; }
        [XmlAttribute(AttributeName = "merge_composites")]
        public string Merge_composites { get; set; }
    }

    [XmlRoot(ElementName = "clashtests")]
    public class Clashtests
    {
        [XmlElement(ElementName = "clashtest")]
        public Clashtest Clashtest { get; set; }
    }

    [XmlRoot(ElementName = "batchtest")]
    public class Batchtest
    {
        [XmlElement(ElementName = "clashtests")]
        public Clashtests Clashtests { get; set; }
        [XmlElement(ElementName = "selectionsets")]
        public string Selectionsets { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "internal_name")]
        public string Internal_name { get; set; }
        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }
    }

    [XmlRoot(ElementName = "exchange")]
    public class Exchange
    {
        [XmlElement(ElementName = "batchtest")]
        public Batchtest Batchtest { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }
        [XmlAttribute(AttributeName = "filename")]
        public string Filename { get; set; }
        [XmlAttribute(AttributeName = "filepath")]
        public string Filepath { get; set; }
    }

}
