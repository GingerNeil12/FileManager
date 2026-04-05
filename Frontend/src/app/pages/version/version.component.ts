import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { environment } from '../../../environments/environment';
import { ToastComponent } from '../../core/components/toast/toast.component';
import { ServiceInfo } from '../../core/models/service-info.model';
import { BackendVersionService } from '../../core/services/backend-version.service';

@Component({
  selector: 'app-version',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './version.component.html',
  imports: [ToastComponent],
})
export class VersionComponent implements OnInit {
  private readonly backendVersionService = inject(BackendVersionService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly services = signal<ServiceInfo[]>([
    { name: environment.appName, version: environment.appVersion },
  ]);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.backendVersionService
      .getServiceInfo()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (info) => this.services.update((current) => [...current, info]),
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          const backendServiceInfoDefault: ServiceInfo = {
            name: 'filemanager-backend',
            version: 'unknown',
            healthStatus: 'Unhealthy'
          };
          this.services.update((current) => [...current, backendServiceInfoDefault])
        },
      });
  }

  protected isHealthy(status: string): boolean {
    return status.toLowerCase() === 'healthy';
  }

  protected isCardHealthy(healthStatus: string | undefined): boolean {
    return !healthStatus || this.isHealthy(healthStatus);
  }

  protected isUnknown(value: string): boolean {
    return value.toLowerCase() === 'unknown';
  }

  protected handleToastDismissed(): void {
    this.errorMessage.set(null);
  }
}
