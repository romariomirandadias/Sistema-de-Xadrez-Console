using System;
using System.Collections.Generic;
using  tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get;private set; }
        public int turno { get;private set; }
        public Cor jogadorAtual { get; private set ;}
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }


        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            pecas = new HashSet<Peca>() ;
            capturadas = new HashSet<Peca>();
            colocarPecas();
        }



        public Peca executarMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQtdMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if (pecaCapturada!=null)
            {
                capturadas.Add(pecaCapturada);
            }
            return pecaCapturada;
        }

        public void realizaJogada(Posicao origem,Posicao destino)
        {
         Peca pecaCapturada=executarMovimento(origem, destino);
            if (estaEmCheque(jogadorAtual))
            {
                desfazerMovimento(origem,destino,pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em cheque !");
            }
            if (estaEmCheque(pecaAdversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }

            if (testarChequeMate(pecaAdversaria(jogadorAtual)))
            {
                terminada = true;
            }
            else 
            {
                turno++;
                mudaJogador();
            }
        }

        private void desfazerMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQtdMovimentos();
            if (pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);
        }

        public void validarPosicaoOrigem(Posicao pos)
        {
            if (tab.peca(pos)==null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida !");
            }
            if (jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua  !");
            }
            if (!tab.peca(pos).existeMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida !");
            }
        }

        public void validarPosicaoDeDestino(Posicao origem,Posicao destino)
        {
            if (!tab.peca(origem).podeMoverPara(destino))
            {
                throw new TabuleiroException ("Posição de destino inválida !");
            }
        }

        private void mudaJogador()
        {
            if (jogadorAtual==Cor.Branca)
            {
                jogadorAtual = Cor.Preta;
            }
            else
            {
                jogadorAtual = Cor.Branca;
            }
        }

        private Cor pecaAdversaria(Cor cor)
        {
            if (cor == Cor.Branca)
            {
                return Cor.Preta;
            }
            else
            {
                return Cor.Branca;
            }
        }

        private Peca rei(Cor cor)
        {
            foreach (Peca x in pecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool estaEmCheque(Cor cor)
        {
            Peca r = rei(cor);
            if (r==null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + "no tabuleiro !");
            }
            foreach (Peca x in pecasEmJogo(pecaAdversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if (mat[r.posicao.linha,r.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testarChequeMate(Cor cor)
        {
            if (!estaEmCheque(cor))
            {
                return false;
            }
            foreach (Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for (int i=0;i< tab.linhas;i++)
                {
                    for (int j=0;j< tab.colunas;j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao destino = new Posicao(i, j);
                            Posicao origem = x.posicao;
                            Peca pecaCapturada = executarMovimento(origem, destino);
                            bool testeCheque = estaEmCheque(cor);
                            desfazerMovimento(origem, destino, pecaCapturada);
                            if (!testeCheque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> pecas = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if (x.cor==cor)
                {
                    pecas.Add(x);
                }
            }
            return pecas;
        }

        public HashSet<Peca> pecasEmJogo(Cor cor)
        {
            HashSet<Peca> nPecas = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    nPecas.Add(x);
                }
            }
            nPecas.ExceptWith(pecasCapturadas(cor));
            return nPecas;
        }

        public void colocarNovaPeca(char coluna,int linha,Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna,linha).toPosicao());
            pecas.Add(peca);
        }

        private void colocarPecas()
        {
            colocarNovaPeca('b', 8, new Torre(tab, Cor.Preta));
            colocarNovaPeca('a', 8, new Rei(tab, Cor.Preta));

            colocarNovaPeca('c',1,new Torre(tab, Cor.Branca));
            colocarNovaPeca('d',1,new Rei(tab, Cor.Branca));
            colocarNovaPeca('h', 7, new Torre(tab, Cor.Branca));
            colocarNovaPeca('b', 1, new Torre(tab, Cor.Branca));


        }

    }
}
