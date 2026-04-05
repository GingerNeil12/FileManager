import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, AsyncPipe],
  templateUrl: './navbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  protected readonly auth = inject(AuthService);
  private readonly elementRef = inject(ElementRef);

  protected readonly isMenuOpen = signal(false);

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isMenuOpen() && !this.elementRef.nativeElement.contains(event.target)) {
      this.isMenuOpen.set(false);
    }
  }

  protected toggleMenu(): void {
    this.isMenuOpen.update((open) => !open);
  }

  protected login(): void {
    this.auth.loginWithRedirect();
  }

  protected logout(): void {
    this.auth.logout({ logoutParams: { returnTo: window.location.origin } });
  }
}
