import { Component, signal } from '@angular/core';
import { formatDate } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/services/auth-service';
import { MatStepperModule } from '@angular/material/stepper';


@Component({
  selector: 'app-register',
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatButtonModule,
    MatSnackBarModule,
    MatStepperModule,
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar,
  ) {
    const maxDate = new Date();
    maxDate.setFullYear(maxDate.getFullYear() - 18);
    this.maxDate = formatDate(maxDate, 'yyyy-MM-dd', 'hu-HU');
  }

  email = '';
  password = '';
  confirmPassword = '';
  firstName = '';
  lastName = '';
  birthDay = '';
  minDate = '1900-01-01';
  maxDate: string;
  gender = '';
  wantsRelationship = false;
  wantsFriendship = false;
  interestedinWoman = false;
  interestedinMan = false;
  mbtiCode: number | null = null;

  isLoading = signal(false);
  errorMessage = signal('');

  public sendRegisterCredentials(form: NgForm): void {
    if (this.isLoading()) {
      return;
    }

    this.errorMessage.set('');
    form.form.markAllAsTouched();

    if (form.invalid || this.password !== this.confirmPassword) {
      return;
    }

    if (!/^[0-9]{4}-[0-9]{2}-[0-9]{2}$/.test(this.birthDay) ||
        this.birthDay < this.minDate || this.birthDay > this.maxDate) {
      this.errorMessage.set(`Enter a date from ${this.minDate} to ${this.maxDate}.`);
      return;
    }

    if (this.gender !== 'male' && this.gender !== 'female') {
      this.errorMessage.set('Please select your gender.');
      return;
    }

    if (this.mbtiCode === null || !Number.isInteger(this.mbtiCode)) {
      this.errorMessage.set('Enter a whole number for the MBTI code.');
      return;
    }

    if (!this.firstName.trim() || !this.lastName.trim()) {
      this.errorMessage.set('Names cannot contain only spaces.');
      return;
    }

    this.isLoading.set(true);

    this.authService
      .register({
        email: this.email.trim(),
        password: this.password,
        firstName: this.firstName.trim(),
        lastName: this.lastName.trim(),
        birthDay: `${this.birthDay}T00:00:00`,
        gender: this.gender,
        wantsRelationship: this.wantsRelationship,
        wantsFriendship: this.wantsFriendship,
        interestedinWoman: this.interestedinWoman,
        interestedinMan: this.interestedinMan,
        mbtiCode: this.mbtiCode,
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Registration successful! Please sign in.', 'Close', {
            duration: 5000,
          });

          void this.router.navigateByUrl('/login');
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.getErrorMessage(error));
        },
      });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    const body = error.error;

    if (Array.isArray(body)) {
      const message = body
        .filter((item) => typeof item?.description === 'string')
        .map((item) => item.description)
        .join(' ');

      if (message) {
        return message;
      }
    }

    if (body?.errors && typeof body.errors === 'object') {
      const message = Object.values(body.errors)
        .flat()
        .filter((value) => typeof value === 'string')
        .join(' ');

      if (message) {
        return message;
      }
    }

    return error.status === 0
      ? 'The server could not be reached.'
      : 'Registration failed. Please try again.';
  }
}
