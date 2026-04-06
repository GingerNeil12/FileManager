import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroCloudArrowUp, heroLink, heroShieldCheck } from '@ng-icons/heroicons/outline';
import { heroFolderOpenSolid } from '@ng-icons/heroicons/solid';
import { combineLatest, filter, take } from 'rxjs';

@Component({
  selector: 'app-landing',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css',
  imports: [NgIcon],
  providers: [provideIcons({ heroFolderOpenSolid, heroCloudArrowUp, heroLink, heroShieldCheck })],
})
export class LandingComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    combineLatest([this.auth.isAuthenticated$, this.auth.isLoading$])
      .pipe(
        filter(([, isLoading]) => !isLoading),
        take(1)
      )
      .subscribe(([isAuthenticated]) => {
        if (isAuthenticated) {
          this.router.navigate(['/dashboard']);
        }
      });
  }

  protected handleLogin(): void {
    this.auth.loginWithRedirect();
  }
}
