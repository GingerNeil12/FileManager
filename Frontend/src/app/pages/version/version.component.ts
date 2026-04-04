import { ChangeDetectionStrategy, Component } from '@angular/core';

import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-version',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './version.component.html',
})
export class VersionComponent {
  protected readonly appName = environment.appName;
  protected readonly appVersion = environment.appVersion;
}
