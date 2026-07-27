import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { PaymentsMatching } from './payments-matching';
import { RunMatchResponse } from './models';

describe('PaymentsMatching', () => {
  let fixture: ComponentFixture<PaymentsMatching>;
  let component: PaymentsMatching;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentsMatching],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentsMatching);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  function makeCsvFile(name: string): File {
    return new File(['orderId,amount,currency\nORD-1,100,INR\n'], name, { type: 'text/csv' });
  }

  it('shows an error and does not call the API when a file is missing', () => {
    component.runMatch();

    expect((component as any).errorMessage()).toContain('both a System CSV and a Provider CSV');
    httpMock.expectNone(() => true);
  });

  it('posts both files and stores the response on success', () => {
    (component as any).systemFile.set(makeCsvFile('system.csv'));
    (component as any).providerFile.set(makeCsvFile('provider.csv'));

    component.runMatch();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/matches/run`);
    expect(req.request.method).toBe('POST');

    const response: RunMatchResponse = {
      batchId: 'batch-1',
      summary: { total: 1, matched: 1, onlySystem: 0, onlyProvider: 0, amountMismatch: 0 },
      items: [],
    };
    req.flush(response);

    expect((component as any).result()).toEqual(response);
    expect((component as any).errorMessage()).toBeNull();
  });

  it('surfaces the backend error message when the request fails', () => {
    (component as any).systemFile.set(makeCsvFile('system.csv'));
    (component as any).providerFile.set(makeCsvFile('provider.csv'));

    component.runMatch();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/matches/run`);
    req.flush({ message: 'bad currency' }, { status: 400, statusText: 'Bad Request' });

    expect((component as any).errorMessage()).toBe('bad currency');
  });
});
