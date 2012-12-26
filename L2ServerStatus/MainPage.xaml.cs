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
using Microsoft.Phone.Reactive;

namespace L2ServerStatus
{
	public partial class MainPage
	{
		public MainPage( )
		{
			InitializeComponent( );
			InitializeServersComponents( );
		}

		private void InitializeServersComponents( )
		{
			foreach ( var serverName in Config.ServerNames )
			{
				Grid grid = new Grid { Margin = new Thickness( 20 ), Name = "grid" + serverName };
				grid.ColumnDefinitions.Add( new ColumnDefinition { Width = new GridLength( 1, GridUnitType.Auto ) } );
				grid.ColumnDefinitions.Add( new ColumnDefinition { Width = new GridLength( 1, GridUnitType.Star ) } );
				grid.ColumnDefinitions.Add( new ColumnDefinition { Width = new GridLength( 1, GridUnitType.Auto ) } );

				grid.RowDefinitions.Add( new RowDefinition( ) );
				grid.RowDefinitions.Add( new RowDefinition( ) );

				TextBlock nameTextBlock = new TextBlock { FontSize = 30, Text = serverName };

				ProgressBar progressBar = new ProgressBar( );
				Grid.SetColumn( progressBar, 1 );
				progressBar.Name = "progressBar" + serverName;
				progressBar.Visibility = Visibility.Collapsed;
				progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
				progressBar.VerticalAlignment = VerticalAlignment.Stretch;

				Rectangle rectangle = new Rectangle( );
				Grid.SetColumn( rectangle, 2 );
				Grid.SetRowSpan( rectangle, 2 );
				rectangle.Name = "status" + serverName;
				rectangle.MinWidth = 50;
				rectangle.Fill = new SolidColorBrush( Colors.Gray );

				TextBlock playersOnlineTextBlock = new TextBlock( );
				Grid.SetRow( playersOnlineTextBlock, 1 );
				Grid.SetColumnSpan( playersOnlineTextBlock, 3 );
				playersOnlineTextBlock.Name = "population" + serverName;
				playersOnlineTextBlock.Text = string.Format( PopulationFormat, "----" );

				grid.Children.Add( playersOnlineTextBlock );
				grid.Children.Add( nameTextBlock );
				grid.Children.Add( rectangle );
				grid.Children.Add( progressBar );

				ContentPanel.Children.Add( grid );
				_grids.Add( serverName, grid );
			}
		}

		private void OnRefreshButtonClick( object sender, RoutedEventArgs e )
		{
			OnRefreshStarted( );

			var stringDownloader = new StringDownloader( );
			stringDownloader.DownloadFromUrl( Config.StatusUrl, OnDownloadSucceeded, OnDownloadException );
		}

		private void OnRefreshStarted( )
		{
			foreach ( var serverName in Config.ServerNames )
			{
				ProgressBar bar = GetGridChild<ProgressBar>( serverName, "progressBar" );
				bar.Visibility = Visibility.Visible;
				bar.IsIndeterminate = true;
			}
		}

		private void OnRefreshFinished( )
		{
			foreach ( var serverName in Config.ServerNames )
			{
				ProgressBar bar = GetGridChild<ProgressBar>( serverName, "progressBar" );
				bar.Visibility = Visibility.Collapsed;
				bar.IsIndeterminate = false;
			}
		}

		private T GetGridChild<T>( string serverName, string childNamePrefix )
		{
			Grid mainGrid = _grids[serverName];
			return ( T )mainGrid.FindName( childNamePrefix + serverName );
		}

		private void OnDownloadException( Exception obj )
		{
			MessageBox.Show( "There was a problem contacting the server. Please check your Internet Connection and try again later." );
			OnRefreshFinished( );
		}
		private void OnDownloadSucceeded( string obj )
		{
			HtmlDocument document = new HtmlDocument( );
			document.LoadHtml( obj );

			var root = document.DocumentNode;
			var serverNodes = root.SelectNodes( "html/body//div[@id = 'top']//table[@class = 'server-status tablesorter']//tr" );
			// skip table header + login server
			for ( int i = 2; i < serverNodes.Count; ++i )
			{
				var serverDetailNodes = serverNodes[i].SelectNodes( "td" );
				var serverName = serverDetailNodes[0].InnerText;
				var status = serverDetailNodes[1].ChildNodes["span"].InnerText;
				var population = serverDetailNodes[2].ChildNodes["span"].InnerText;

				Scheduler.Dispatcher.Schedule( ( ) =>
				{
					UpdateServerInfo( serverName, status == "Online", population );
				} );
			}

			Scheduler.Dispatcher.Schedule( OnRefreshFinished );
		}

		private void UpdateServerInfo( string serverName, bool online, string population )
		{
			Rectangle status = GetGridChild<Rectangle>( serverName, "status" );
			status.Fill = new SolidColorBrush( online ? Colors.Green : Colors.Red );

			TextBlock players = GetGridChild<TextBlock>( serverName, "population" );
			players.Text = string.Format( PopulationFormat, population );
		}

		private const string PopulationFormat = "Population: {0}";

		private readonly IDictionary<string, Grid> _grids = new Dictionary<string, Grid>( );
	}

	class WebRequestState
	{
		public WebRequest WebRequest;
		public string ServerName;
	}
}