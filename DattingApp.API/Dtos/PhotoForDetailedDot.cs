using System;

namespace DattingApp.API.Dtos
{
    public class PhotoForDetailedDot
    {
        public int ID {get;set;}
        public string Url {get;set;}
        public string Description {get;set;}
        public DateTime DateAdded {get;set;}
        public bool IsMain {get;set;}
    }
}