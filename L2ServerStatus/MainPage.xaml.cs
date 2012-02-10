/*
   Copyright (C) 2012 by Timotei Dolean <timotei21@gmail.com>

   Part of the Lineage 2 Server Status Project https://github.com/timotei/l2_server_status

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2 of the License, or
   (at your option) any later version.
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY.

   See the COPYING file for more details.
*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HtmlAgilityPack;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;

namespace L2ServerStatus
{
	public partial class MainPage : PhoneApplicationPage
	{
		// Constructor
		public MainPage( )
		{
			InitializeComponent( );
			InitializeServersComponents( );
		}

		private void InitializeServersComponents( )
		{
			foreach ( var serverName in Config.ServerNames ) {
				Grid grid = new Grid( );
				grid.Margin = new Thickness( 20 );
				grid.Name = "grid" + serverName;
				grid.ColumnDefinitions.Add( new ColumnDefinition( ) { Width = new GridLength( 1, GridUnitType.Auto ) } );
				grid.ColumnDefinitions.Add( new ColumnDefinition( ) { Width = new GridLength( 1, GridUnitType.Star ) } );
				grid.ColumnDefinitions.Add( new ColumnDefinition( ) { Width = new GridLength( 1, GridUnitType.Auto ) } );

				grid.RowDefinitions.Add( new RowDefinition( ) );
				grid.RowDefinitions.Add( new RowDefinition( ) );

				TextBlock nameTextBlock = new TextBlock( );
				nameTextBlock.FontSize = 30;
				nameTextBlock.Text = serverName;

				ProgressBar progressBar = new ProgressBar( );
				Grid.SetColumn( progressBar, 1 );
				progressBar.Name = "progressBar" + serverName;
				progressBar.Visibility = Visibility.Collapsed;
				progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
				progressBar.VerticalAlignment = VerticalAlignment.Stretch;

				Rectangle rectangle = new Rectangle( );
				Grid.SetColumn( rectangle, 2 );
				rectangle.Name = "status" + serverName;
				rectangle.MinWidth = 50;
				rectangle.Fill = new SolidColorBrush( Colors.Gray );

				TextBlock playersOnlineTextBlock = new TextBlock( );
				Grid.SetRow( playersOnlineTextBlock, 1 );
				Grid.SetColumnSpan( playersOnlineTextBlock, 3 );
				playersOnlineTextBlock.Name = "players" + serverName;
				playersOnlineTextBlock.Text = string.Format( PlayersOnlineFormat, "----" );

				TextBlock pingTextBlock = new TextBlock( );
				Grid.SetRow( pingTextBlock, 1 );
				Grid.SetColumnSpan( pingTextBlock, 3 );
				pingTextBlock.Name = "ping" + serverName;
				pingTextBlock.Text = string.Format( PingFormat, "----" );
				pingTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

				grid.Children.Add( playersOnlineTextBlock );
				grid.Children.Add( pingTextBlock );
				grid.Children.Add( nameTextBlock );
				grid.Children.Add( rectangle );
				grid.Children.Add( progressBar );

				ContentPanel.Children.Add( grid );
				_grids.Add( serverName, grid );
			}
		}

		private void OnRefreshButtonClick( object sender, RoutedEventArgs e )
		{
			_responseErrorOcurred = false;
			TimeSpan timeSpan = TimeSpan.FromMilliseconds( 0 );
			foreach ( var serverName in Config.ServerNames ) {
				OnRefreshStarted( serverName );

				string name = serverName;
				Scheduler.NewThread.Schedule( ( ) => RefreshServerStatus( name ), timeSpan );
				timeSpan = timeSpan.Add( TimeSpan.FromSeconds( 1 ) );
			}
		}

		private void OnRefreshStarted( string serverName )
		{
			ProgressBar bar = GetGridChild<ProgressBar>( serverName, "progressBar" );
			bar.Visibility = Visibility.Visible;
			bar.IsIndeterminate = true;
		}

		private void OnRefreshFinished( string serverName )
		{
			ProgressBar bar = GetGridChild<ProgressBar>( serverName, "progressBar" );
			bar.Visibility = Visibility.Collapsed;
			bar.IsIndeterminate = false;
		}

		private T GetGridChild<T>( string serverName, string childNamePrefix )
		{
			Grid mainGrid = _grids[serverName];
			return ( T ) mainGrid.FindName( childNamePrefix + serverName );
		}

		private void RefreshServerStatus( string serverName )
		{
			string url = string.Format( Config.UrlFormat, serverName );

			WebRequest request = WebRequest.Create( url );

			WebRequestState state = new WebRequestState {
				ServerName = serverName,
				WebRequest = request
			};

			request.BeginGetResponse( OnRequestResponse, state );
		}

		private void OnRequestResponse( IAsyncResult ar )
		{
			var state = ( WebRequestState ) ar.AsyncState;
			WebResponse response = null;
			try {
				response = state.WebRequest.EndGetResponse( ar );
			}
			catch ( Exception e ) {
				if ( !_responseErrorOcurred ) {
					Scheduler.Dispatcher.Schedule( ( ) => MessageBox.Show( "There was a problem contacting the server. Please check your Internet Connection." ) );
					_responseErrorOcurred = true;
				}
				Scheduler.Dispatcher.Schedule( ( ) => OnRefreshFinished( state.ServerName ) );
				return;
			}
			HtmlDocument document = new HtmlDocument( );
			document.Load( response.GetResponseStream( ) );

			var root = document.DocumentNode;

			var statusNode = root.SelectSingleNode( "html/body/table[@class='c5']/tr/td[not(@class)]/div" );
			var style = statusNode.Attributes["style"].Value;

			var serverDetailsNode = root.SelectNodes( "html/body/table[@align='center' and position() =2]/tr/td" );
			var players = serverDetailsNode[1].InnerText;
			var ping = serverDetailsNode[6].InnerText;

			Scheduler.Dispatcher.Schedule( ( ) => {
				UpdateServerInfo( state.ServerName, style, players, ping );
				OnRefreshFinished( state.ServerName );
			} );
		}

		private void UpdateServerInfo( string serverName, string styleText, string playersText, string pingText )
		{
			Rectangle status = GetGridChild<Rectangle>( serverName, "status" );
			Color color = ParseCSSBackgroundColor( styleText );
			status.Fill = new SolidColorBrush( color );

			TextBlock players = GetGridChild<TextBlock>( serverName, "players" );
			players.Text = string.Format( PlayersOnlineFormat, ParsePlayersOnline( playersText ) );

			TextBlock ping = GetGridChild<TextBlock>( serverName, "ping" );
			ping.Text = string.Format( PingFormat, ParsePing( pingText ) );
		}

		private static string ParsePlayersOnline( string playersText )
		{
			return playersText.Split( '/' )[0];
		}

		private static int ParsePing( string pingText )
		{
			const string delayText = "delay: ";
			int start = pingText.IndexOf( delayText, StringComparison.InvariantCulture );
			start += delayText.Length;
			int end = pingText.IndexOf( ' ', start );

			float seconds = float.Parse( pingText.Substring( start, end - start ) );
			return ( int ) ( seconds * 1000f );
		}

		private static Color ParseCSSBackgroundColor( string styleText )
		{
			int tmpStart = styleText.IndexOf( "background-color", StringComparison.InvariantCulture );
			int start = styleText.IndexOf( "#", tmpStart, StringComparison.InvariantCulture ) + 1;
			int end = styleText.IndexOf( ';', start );
			if ( end == -1 ) {
				end = styleText.Length;
			}

			string colorString = styleText.Substring( start, end - start );
			int color = Convert.ToInt32( colorString, 16 );
			return Color.FromArgb(
				255, // ignore alpha.
				( byte ) ( color >> 16 ),
				( byte ) ( color >> 8 ),
				( byte ) color
				);
		}

		private bool _responseErrorOcurred = false;
		private const string PlayersOnlineFormat = "Players online: {0}";
		private const string PingFormat = "Ping: {0} ms";

		private readonly IDictionary<string, Grid>  _grids = new Dictionary<string, Grid>( );
	}

	class WebRequestState
	{
		public WebRequest WebRequest;
		public string ServerName;
	}
}