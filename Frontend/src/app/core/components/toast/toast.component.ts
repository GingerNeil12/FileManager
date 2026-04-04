import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-toast',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './toast.component.html',
})
export class ToastComponent {
  readonly message = input.required<string>();
  readonly dismissed = output<void>();

  protected handleDismiss(): void {
    this.dismissed.emit();
  }
}
