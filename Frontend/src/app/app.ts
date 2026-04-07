import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';

import { AuthService } from '@auth0/auth0-angular';

import { NavbarComponent } from './core/components/navbar/navbar.component';
import { NavPanelComponent } from './core/components/nav-panel/nav-panel.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, NavPanelComponent],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly isAuthenticated = toSignal(inject(AuthService).isAuthenticated$, { initialValue: false });
  protected readonly isNavExpanded = signal(true);
}
