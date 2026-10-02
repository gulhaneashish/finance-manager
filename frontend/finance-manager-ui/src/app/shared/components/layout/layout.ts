import { Component } from '@angular/core';

import { RouterOutlet } from '@angular/router';

import { Sidebar } from '../sidebar/sidebar';

import { Header } from '../header/header';


@Component({
  selector: 'app-layout',

  standalone: true,

  imports: [
    RouterOutlet,
    Sidebar,
    Header
  ],

  templateUrl: './layout.html',

  styleUrl: './layout.css'
})
export class Layout {

  isSidebarOpen = false;


  toggleSidebar(): void {

    this.isSidebarOpen =
      !this.isSidebarOpen;

  }


  closeSidebar(): void {

    this.isSidebarOpen = false;

  }

}