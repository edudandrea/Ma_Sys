import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { AlunosService } from '../Services/AlunosService/Alunosservice';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { CadastroAlunosService } from '../Services/CadastroAlunos/CadastroAlunos.service';
import { CadastroAlunosContext } from '../Services/CadastroAlunos/CadastroAlunos.service';
import * as QRCode from 'qrcode';
import { PagamentosService } from '../Services/PagamentosService/Pagamentos.service';

@Component({
  selector: 'app-CadastroAlunos',
  standalone: true,
  templateUrl: './CadastroAlunos.component.html',
  styleUrls: ['./CadastroAlunos.component.css'],
  imports: [CommonModule, FormsModule],
})
export class CadastroAlunosComponent implements OnInit {
  cpfBusca: string = '';
  cpf: string = '';
  endereco: string = '';
  bairro: string = '';
  cidade: string = '';
  estado: string = '';
  cep: string = '';
  obs: string = '';
  emailBusca: string = '';
  nome: string = '';
  telefone: string = '';
  email: string = '';
  graduacao: string = '';
  redeSocial: string = '';
  dataNascimento: string = '';
  dataCadastro: string = '';
  mostrarFormulario: boolean = false;

  planoId: number = 0;
  planos: any[] = [];
  planoSelecionado: any = null;

  formaPagamentoId: number = 0;
  formasPagamento: any[] = [];
  formaPagamentoSelecionada: any = null;

  isPix: boolean = false;
  showQrCode: boolean = false;
  qrCodePix: string = '';
  pixPayload: string = '';

  aluno: any = null;
  alunoEncontrado: boolean = false;
  mostrarCadastro: boolean = false;

  numeroCartao: string = '';
  validadeCartao: string = '';
  cvvCartao: string = '';

  isCartao: boolean = false;
  bandeiraCartao: string = '';

  cartao = {
    numero: '',
    nome: '',
    validade: '',
    cvv: '',
  };

  constructor(
    private modalService: BsModalService,
    private alunoService: AlunosService,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private spinner: NgxSpinnerService,
    private cadastroAlunosService: CadastroAlunosService,
    private context: CadastroAlunosContext,
    private pgService: PagamentosService,
    private cd: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.context.slug = this.route.snapshot.paramMap.get('academia')!;
    console.log('SLUG:', this.context.slug);
  }

  pesquisarAlunos() {
    this.spinner.show();

    this.cadastroAlunosService.getByCpfEmail(this.cpfBusca, this.emailBusca).subscribe({
      next: (res: any) => {
        this.spinner.hide();
        this.aluno = res;
        this.nome = res.nome;
        this.telefone = res.telefone;
        this.cpf = res.cpf;
        this.endereco = res.endereco;
        this.bairro = res.bairro;
        this.cidade = res.cidade;
        this.estado = res.estado;
        this.cep = res.cep;
        this.email = res.email;
        this.redeSocial = res.redeSocial;
        this.graduacao = res.graduacao;
        this.dataNascimento = res.dataNascimento;
        this.dataCadastro = res.dataCadastro;
        this.obs = res.obs;
        this.alunoEncontrado = true;
        this.carregarFormaPgtos();
        this.cd.detectChanges();
      },
      error: (err) => {
        this.spinner.hide();

        if (err.status === 404) {
          this.toastr.warning('Aluno não encontrado.');
          this.alunoEncontrado = false;
        } else {
          this.toastr.error('Erro ao buscar aluno.');
        }
      },
    });
  }

  onPlanoChange() {
    const plano = this.planos.find((p) => p.id == this.planoId);
    this.planoSelecionado = plano;
    this.showQrCode = false;
    this.qrCodePix = '';
  }

  onFormaPagamentoChange() {
    this.formaPagamentoSelecionada = this.formasPagamento.find((f) => f.id == this.formaPagamentoId);
    const nome = this.formaPagamentoSelecionada?.nome?.toLowerCase() || '';

    this.isPix = nome === 'pix';
    this.isCartao = nome.includes('crédito') || nome.includes('credito') || nome.includes('debito');

    this.showQrCode = false;
    this.qrCodePix = '';
    this.numeroCartao = '';
    this.validadeCartao = '';
    this.cvvCartao = '';
  }

  gerarPix() {
    const valor = this.aluno?.valor || 0;

    this.pgService.gerarPixPublico(valor, this.nome || 'Aluno', this.cidade || 'Cidade').subscribe((res) => {
      this.pixPayload = res.payload;
      this.gerarCodePix(res.payload);
    });
  }

  gerarCodePix(payload: string) {
    QRCode.toDataURL(payload).then((url) => {
      setTimeout(() => {
        this.qrCodePix = url;
        this.showQrCode = true;
        this.cd.detectChanges();
      }, 0);
    });
  }

  copiarPix() {
    if (!this.pixPayload) {
      return;
    }

    navigator.clipboard.writeText(this.pixPayload);
    this.toastr.success('Código PIX copiado. Cole no aplicativo do seu banco.');
  }

  carregarFormaPgtos() {
    this.spinner.show();

    this.pgService.getFormaPagamentosPublico(this.context.slug).subscribe({
      next: (res) => {
        this.spinner.hide();
        this.formasPagamento = res.map((p) => ({
          ...p,
          menuAberto: false,
        }));
      },
      error: (err) => {
        this.spinner.hide();
        console.error(err);
        this.toastr.error('Erro ao carregar formas de pagamento.', 'Erro');
      },
    });
  }

  pagarCartao() {
    if (!this.aluno?.alunoId || !this.aluno?.matriculaId || !this.aluno?.planoId) {
      this.toastr.error('Não foi possível identificar a matrícula para o pagamento.');
      return;
    }

    if (!this.formaPagamentoId) {
      this.toastr.warning('Selecione a forma de pagamento.');
      return;
    }

    if (!this.cartao.numero || !this.cartao.nome || !this.cartao.validade || !this.cartao.cvv) {
      this.toastr.warning('Preencha os dados do cartão.');
      return;
    }

    const payload = {
      slug: this.context.slug,
      alunoId: this.aluno.alunoId,
      matriculaId: this.aluno.matriculaId,
      planoId: this.aluno.planoId,
      formaPagamentoId: Number(this.formaPagamentoId),
      valor: this.aluno?.valor,
      cartao: this.cartao,
    };

    this.spinner.show();

    this.pgService.pagarCartaoPublico(payload).subscribe({
      next: () => {
        this.spinner.hide();
        this.toastr.success('Pagamento realizado com sucesso!');
      },
      error: (err) => {
        this.spinner.hide();
        const message = err?.error?.message || 'Não foi possível processar o pagamento no cartão.';
        this.toastr.error(message);
      },
    });
  }
}
