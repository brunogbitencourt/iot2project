import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';
import { TokenStoreService } from '../../infrastructure/storage/TokenStoreService';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private store: TokenStoreService) {}
  intercept(req: HttpRequest<any>, next: HttpHandler) {
    const token = this.store.getAccess();
    if (token) {
      req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }
    return next.handle(req);
  }
}
