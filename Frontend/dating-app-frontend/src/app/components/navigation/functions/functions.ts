import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-functions',
  imports: [RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './functions.html',
  styleUrl: './functions.scss',
})
export class Functions {}
