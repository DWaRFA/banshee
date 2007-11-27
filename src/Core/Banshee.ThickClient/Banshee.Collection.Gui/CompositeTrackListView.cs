//
// CompositeTrackListView.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;

using Gtk;
using Mono.Unix;

using Hyena.Data;
using Hyena.Data.Gui;

using Banshee.Gui;
using Banshee.ServiceStack;
using Banshee.Collection;
using Banshee.Configuration;

namespace Banshee.Collection.Gui
{
    public class CompositeTrackListView : VBox
    {
        private ArtistListView artist_view;
        private AlbumListView album_view;
        private TrackListView track_view;
        
        private ArtistListModel artist_model;
        private AlbumListModel album_model;
        private TrackListModel track_model;
        
        private ScrolledWindow artist_scrolled_window;
        private ScrolledWindow album_scrolled_window;
        private ScrolledWindow track_scrolled_window;
        
        private Paned container;
        private Widget browser_container;
        private InterfaceActionService action_service;
        private ActionGroup browser_view_actions;
        private uint action_merge_id;
        
        private static string menu_xml = @"
            <ui>
              <menubar name=""MainMenu"">
                <menu name=""ViewMenu"" action=""ViewMenuAction"">
                  <placeholder name=""BrowserViews"">
                    <menuitem name=""BrowserVisible"" action=""BrowserVisibleAction"" />
                    <separator />
                    <menuitem name=""BrowserTop"" action=""BrowserTopAction"" />
                    <menuitem name=""BrowserLeft"" action=""BrowserLeftAction"" />
                    <separator />
                  </placeholder>
                </menu>
              </menubar>
            </ui>
        ";
        
        public CompositeTrackListView ()
        {
            string position = BrowserPosition.Get ();
            if (position == "top") {
                LayoutTop ();
            } else {
                LayoutLeft ();
            }
            
            if (ServiceManager.Contains ("InterfaceActionService")) {
                action_service = ServiceManager.Get<InterfaceActionService> ("InterfaceActionService");
                
                browser_view_actions = new ActionGroup ("BrowserView");
                
                browser_view_actions.Add (new RadioActionEntry [] {
                    new RadioActionEntry ("BrowserLeftAction", null, 
                        Catalog.GetString ("Browser On Left"), null,
                        Catalog.GetString ("Show the artist/album browser to the left of the track list"), 0),
                    
                    new RadioActionEntry ("BrowserTopAction", null,
                        Catalog.GetString ("Browser On Top"), null,
                        Catalog.GetString ("Show the artist/album browser above the track list"), 1),
                }, position == "top" ? 1 : 0, OnViewModeChanged);
                
                browser_view_actions.Add (new ToggleActionEntry [] {
                    new ToggleActionEntry ("BrowserVisibleAction", null,
                        Catalog.GetString ("Show Browser"), "<control>B",
                        Catalog.GetString ("Show or hide the artist/album browser"), 
                        OnToggleBrowser, BrowserVisible.Get ())
                });
                
                action_service.AddActionGroup (browser_view_actions);
                action_merge_id = action_service.UIManager.NewMergeId ();
                action_service.UIManager.AddUiFromString (menu_xml);
            }
            
            NoShowAll = true;
        }
        
        private void BuildCommon ()
        {
            artist_view = new ArtistListView();
            album_view = new AlbumListView();
            track_view = new TrackListView();
        
            artist_view.HeaderVisible = false;
            album_view.HeaderVisible = false;
            
            artist_view.Selection.Changed += OnBrowserViewSelectionChanged;
            album_view.Selection.Changed += OnBrowserViewSelectionChanged;
            
            artist_scrolled_window = new ScrolledWindow();
            artist_scrolled_window.Add(artist_view);
            artist_scrolled_window.HscrollbarPolicy = PolicyType.Automatic;
            artist_scrolled_window.VscrollbarPolicy = PolicyType.Automatic;
            
            album_scrolled_window = new ScrolledWindow();
            album_scrolled_window.Add(album_view);
            album_scrolled_window.HscrollbarPolicy = PolicyType.Automatic;
            album_scrolled_window.VscrollbarPolicy = PolicyType.Automatic;
            
            track_scrolled_window = new ScrolledWindow();
            track_scrolled_window.Add(track_view);
            track_scrolled_window.HscrollbarPolicy = PolicyType.Automatic;
            track_scrolled_window.VscrollbarPolicy = PolicyType.Automatic;
            
            artist_view.Model = artist_model;
            album_view.Model = album_model;
            track_view.Model = track_model;
        }
        
        private void Clean ()
        {
            if (artist_view != null) {
                artist_view.Destroy ();
            }
            
            if (album_view != null) {
                album_view.Destroy ();
            }
            
            if (track_view != null) {
                track_view.Destroy ();
            }
            
            if (container != null) {
                container.Destroy ();
            }
            
            BuildCommon ();
        }

        private void LayoutLeft ()
        {
            Clean ();
            
            container = new HPaned ();
            VPaned artist_album_box = new VPaned ();
            
            artist_album_box.Add1 (artist_scrolled_window);
            artist_album_box.Add2 (album_scrolled_window);

            artist_album_box.Position = 350;
            
            container.Add1 (artist_album_box);
            container.Add2 (track_scrolled_window);
            
            browser_container = artist_album_box;
            
            container.Position = 275;
            ShowPack ();
        }
        
        private void LayoutTop ()
        {
            Clean ();
            
            container = new VPaned ();
            HBox artist_album_box = new HBox ();
            artist_album_box.Spacing = 10;
            
            artist_album_box.PackStart (artist_scrolled_window, true, true, 0);
            artist_album_box.PackStart (album_scrolled_window, true, true, 0);
            
            container.Add1 (artist_album_box);
            container.Add2 (track_scrolled_window);
            
            browser_container = artist_album_box;
            
            container.Position = 175;
            ShowPack ();
        }
        
        private void ShowPack ()
        {
            PackStart (container, true, true, 0);
            NoShowAll = false;
            ShowAll ();
            NoShowAll = true;
            browser_container.Visible = BrowserVisible.Get ();
        }
        
        private void OnViewModeChanged (object o, ChangedArgs args)
        {
            if (args.Current.Value == 0) {
                LayoutLeft ();
                BrowserPosition.Set ("left");
            } else {
                LayoutTop ();
                BrowserPosition.Set ("top");
            }
        }
                
        private void OnToggleBrowser (object o, EventArgs args)
        {
            ToggleAction action = (ToggleAction)o;
            artist_view.Selection.Clear ();
            browser_container.Visible = action.Active;
            BrowserVisible.Set (action.Active);
        }
        
        protected virtual void OnBrowserViewSelectionChanged(object o, EventArgs args)
        {
            Hyena.Collections.Selection selection = (Hyena.Collections.Selection)o;
            object view = selection.Owner;
            TrackListModel model = track_view.Model as TrackListModel;
            
            if(selection.Count == 1 && selection.Contains(0) || selection.AllSelected) {
                if(view is ArtistListView && model != null) {
                    model.ArtistInfoFilter = null;
                } else if(view is AlbumListView && model != null) {
                    model.AlbumInfoFilter = null;
                }
                return;
            } else if(selection.Count == 0) {
                selection.Select(0);
            } else if(selection.Count > 0 && !selection.AllSelected) {
                selection.Unselect(0);
            }
            
            if(view is ArtistListView) {
                ArtistInfo [] artists = new ArtistInfo[selection.Count];
                int i = 0;
            
                foreach(int row_index in artist_view.Selection) {
                    artists[i++] = artist_view.Model[row_index];
                }
            
                model.ArtistInfoFilter = artists;
                ((AlbumListModel)album_view.Model).ArtistInfoFilter = artists;
                
                album_view.Selection.Select(0);
                album_view.Selection.Clear();
            } else if(view is AlbumListView) {
                AlbumInfo [] albums = new AlbumInfo[selection.Count];
                int i = 0;
            
                foreach(int row_index in album_view.Selection) {
                    albums[i++] = album_view.Model[row_index];
                }
            
                model.AlbumInfoFilter = albums;
            }
            
            track_view.Selection.Clear ();
        }
        
        public TrackListView TrackView {
            get { return track_view; }
        }
        
        public ArtistListView ArtistView {
            get { return artist_view; }
        }
        
        public AlbumListView AlbumView {
            get { return album_view; }
        }
        
        public TrackListModel TrackModel {
            get { return (TrackListModel)track_view.Model; }
            set {
                track_model = value;
                track_view.Model = value; 
            }
        }
        
        public ArtistListModel ArtistModel {
            get { return (ArtistListModel)artist_view.Model; }
            set {
                artist_model = value;
                artist_view.Model = value; 
            }
        }
        
        public AlbumListModel AlbumModel {
            get { return (AlbumListModel)album_view.Model; }
            set {
                album_model = value;
                album_view.Model = value;
            }
        }
        
        public static readonly SchemaEntry<bool> BrowserVisible = new SchemaEntry<bool> (
            "browser", "visible",
            true,
            "Artist/Album Browser Visibility",
            "Whether or not to show the Artist/Album browser"
        );
        
        public static readonly SchemaEntry<string> BrowserPosition = new SchemaEntry<string> (
            "browser", "position",
            "left",
            "Artist/Album Browser Position",
            "The position of the Artist/Album browser; either 'top' or 'left'"
        );
    }
}
