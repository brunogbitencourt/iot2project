import { Component, OnInit } from '@angular/core';
import { CommonModule }      from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { UsersApiService, UserDto } from '../../../infrastructure/http/UsersApiService';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-usuarios-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './usuarios-page.component.html',
  styleUrls: ['./usuarios-page.component.scss']
})
export class UsuariosPageComponent implements OnInit {
  form: FormGroup;
  users: UserDto[] = [];
  loading = false;
  editingId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private api: UsersApiService
  ) {
    this.form = this.fb.group({
      FullName:      ['', Validators.required],
      Email:         ['', [Validators.required, Validators.email]],
      Password:      ['', Validators.required],
      UserProfileId: [null, Validators.required]
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading = true;
    this.api.getAll()
      .pipe(finalize(() => this.loading = false))
      .subscribe(list => this.users = list);
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    const dto = this.form.value;  // já tem FullName, Email, Password, UserProfileId
    const op$ = this.editingId != null
      ? this.api.update(this.editingId, dto)
      : this.api.create(dto);

    op$
      .pipe(finalize(() => this.loading = false))
      .subscribe(() => {
        this.resetForm();
        this.loadUsers();
      });
  }

  edit(user: UserDto) {
    this.editingId = user.UserId;
    this.form.setValue({
      FullName:      user.FullName,
      Email:         user.Email,
      Password:      '',                // senha não retorna, deixa em branco
      UserProfileId: user.UserProfileId
    });
  }

  delete(id: number) {
    if (!confirm('Confirma exclusão deste usuário?')) return;
    this.api.delete(id).subscribe(() => this.loadUsers());
  }

  resetForm() {
    this.editingId = null;
    this.form.reset();
  }
}
