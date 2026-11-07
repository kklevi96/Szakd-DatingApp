import { Component } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { Hobby } from '../hobby';

@Component({
  selector: 'app-hobby-dialog',
  imports: [MatDialogModule, MatButtonModule, Hobby],
  templateUrl: './hobby-dialog.html',
  styleUrl: './hobby-dialog.scss',
})
export class HobbyDialog {
  constructor(public dialogRef: MatDialogRef<HobbyDialog>) {}
}
