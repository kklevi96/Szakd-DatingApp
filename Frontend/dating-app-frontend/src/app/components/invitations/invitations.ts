import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-invitations',
  imports: [],
  templateUrl: './invitations.html',
  styleUrl: './invitations.scss',
})
export class Invitations {
  @Input({ required: true }) conversationId = 0;
}
