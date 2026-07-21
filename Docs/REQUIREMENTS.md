# Requisitos do Projeto - [A2-Gestamp-App]

## 1. Visão Geral

**Quam vai usar?**
O sistema vai ser usado pela empresa Gestamp pra inspeção de imagens que julgam a qualidade dos seus produtos.

**O que ele resolve?**
O operador precisa de uma interface melhor e mais intuitiva, controle de quem esta usando o sistema, controle de dados e extradificação.

## 2. Requisitos Funcionais (O que o sistema FAZ)
Use o padrão **RF01**, **RF02**, etc. Seja direto.

- **RF01:** O usuário deve conseguir visualizar as 3 imagens da inspeção atual e as ferramentas julgadas.
- **RF02:** O usuário precisa saber o status do sistema e das comunicações externas.
- **RF03:** O usuário deve ver o status da produção em tempo real (GOOD, NG, ÍNDICES).
- **RF04:** O sistema deve bloquear toda operação caso uma peça seja julgada como NG e solicitar login.
- **RF05:** O usuário pode se cadastrar no sistema caso seja um Administrador.
- **RF06:** O Administrador pode aprovar uma peça NG ou mante-la reprovada após efetuar login.
- **RF07:** O usuário pode somente manter a peça reprovada após efetuar login.
- **RF08:** O sistema deve salvar as 3 imagens caso for uma inspeção NG.
- **RF09:** O sistema deve salvar no banco toda inspeção

## 3. Requisitos Não-Funcionais (Como o sistema SE COMPORTA)
Restrições técnicas, segurança ou performance. Use **RNF01**, **RNF02**.

- **RNF01:** A aplicação deve ser responsiva (funcionar bem em telas de painel pc e desktop).

## 4. Escopo Futuro (O que NÃO vai ter agora - MVP)
Listar o que você **sabe que quer**, mas decidiu cortar da primeira versão para não travar o projeto. Isso evita que você caia na armadilha de abraçar o mundo.
- [ ] Exportar dados para PDF (Fica para a v2)
- [ ] Gráficos avançados (Fica para a v2)
- [ ] Deletar usuários (Fica para a v2)