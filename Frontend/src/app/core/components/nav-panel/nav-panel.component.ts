import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  inject,
  model,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationStart, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroChevronLeft, heroChevronRight, heroCog6Tooth } from '@ng-icons/heroicons/outline';
import { filter } from 'rxjs';

@Component({
  selector: 'app-nav-panel',
  standalone: true,
  imports: [NgIcon, RouterLink],
  providers: [provideIcons({ heroCog6Tooth, heroChevronLeft, heroChevronRight })],
  templateUrl: './nav-panel.component.html',
  styleUrl: './nav-panel.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavPanelComponent implements OnInit {
  readonly isExpanded = model<boolean>(true);

  protected readonly isSettingsOpen = signal(false);

  private readonly router = inject(Router);
  private readonly elementRef = inject(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationStart),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.isSettingsOpen.set(false));
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isSettingsOpen() && !this.elementRef.nativeElement.contains(event.target)) {
      this.isSettingsOpen.set(false);
    }
  }

  protected handleToggleCollapse(): void {
    this.isExpanded.update((v) => !v);
  }

  protected handleToggleSettings(event: MouseEvent): void {
    event.stopPropagation();
    this.isSettingsOpen.update((v) => !v);
  }
}
