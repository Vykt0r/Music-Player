using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Library
{
    public class Playlist
    {
        [XmlElement]
        public List<Track> Tracks { get; set; }

        [XmlElement]
        public string Name { get; set; }

        public Playlist()
        {
            Tracks = new List<Track>();
            Name = "[UNNAMED]";
        }

        public Playlist(string name)
            : this()
        {
            Name = name;
        }

        public Playlist(List<Track> tracks)
            : this()
        {
            Tracks = tracks;
        }

        public Playlist(string name, List<Track> tracks)
            : this(name)
        {
            Tracks = tracks;
        }

        public Playlist(Track[] tracks)
            : this()
        {
            foreach (var track in tracks)
            {
                Tracks.Add(track);
            }
        }

        public Playlist(string name, Track[] tracks)
            : this(tracks)
        {
            Name = name;
        }
    }
}
