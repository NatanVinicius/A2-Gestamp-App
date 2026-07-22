# Domínio - A2-Gestamp-App

## Objetivo

Este documento descreve o funcionamento do negócio da aplicação.

Ele não define arquitetura, banco de dados ou implementação.

Seu objetivo é explicar como o sistema funciona para qualquer desenvolvedor antes do início da implementação.

---

# Visão Geral

O A2-Gestamp-App é responsável por acompanhar inspeções realizadas por um sistema externo de visão.

Cada inspeção representa o julgamento de uma única peça produzida.

O sistema recebe esse resultado, apresenta as informações ao operador e registra todas as decisões tomadas.

O fluxo principal do sistema é baseado nas inspeções.

---

# Entidades

## ProductionShift

Representa um turno de produção.

Não é criado manualmente.

O sistema cria automaticamente um turno quando a primeira inspeção daquele período é recebida.

Regras:

- Sempre possui duração de 8 horas.
- Existem 3 turnos por dia.
- Toda inspeção pertence obrigatoriamente a um turno.
- Nunca podem existir dois turnos para o mesmo período.

Responsabilidades:

- Armazenar estatísticas da produção.
- Consolidar as inspeções realizadas durante o turno.

---

## Inspection

Representa a inspeção de uma única peça.

Cada inspeção possui exatamente um resultado inicial enviado pelo sistema de visão.

Uma inspeção sempre pertence a um único turno.

Responsabilidades:

- Armazenar as imagens da inspeção.
- Armazenar o julgamento recebido.
- Registrar eventual alteração realizada por um administrador.

---

## User

Representa um operador do sistema.

Um usuário pode possuir diferentes níveis de acesso.

Tipos:

- Operador
- Administrador

---

# Conceitos

## GOOD

Peça aprovada automaticamente pelo sistema de inspeção.

Não exige intervenção do operador.

---

## NG

Peça reprovada automaticamente pelo sistema de inspeção.

Exige autenticação de um usuário antes da continuidade da produção.

---

## Julgamento Original

Resultado enviado pelo sistema de inspeção.

Nunca deve ser alterado.

Valores possíveis:

- GOOD
- NG

---

## Julgamento Final

Resultado considerado definitivo pelo sistema.

Pode ser igual ao julgamento original ou alterado por um administrador.

---

# Regras de Negócio

## RG01

Toda inspeção pertence obrigatoriamente a um ProductionShift.

---

## RG02

ProductionShift é criado automaticamente quando necessário.

---

## RG03

ProductionShift possui duração fixa de 8 horas.

---

## RG04

Existem exatamente três turnos por dia.

---

## RG05

Ao receber uma inspeção, o sistema deve localizar o turno correspondente ao horário da inspeção.

Caso ele não exista, deve criá-lo automaticamente.

---

## RG06

Inspeções GOOD não exigem intervenção do operador.

---

## RG07

Inspeções NG bloqueiam o fluxo até que um usuário realize autenticação.

---

## RG08

Usuários comuns podem apenas confirmar a reprovação da peça.

---

## RG09

Somente administradores podem alterar um julgamento NG para GOOD.

---

## RG10

Toda alteração de julgamento deve permanecer registrada para auditoria.

---

## Fluxo Principal

Receber inspeção

↓

Localizar turno

↓

Criar turno (caso necessário)

↓

Vincular inspeção ao turno

↓

Atualizar indicadores

↓

Resultado GOOD?

├── Sim → Finalizar

└── NG

↓

Solicitar login

↓

Administrador?

├── Não → Confirmar NG

└── Sim → Aprovar ou manter NG

↓

Salvar decisão

↓

Aguardar próxima inspeção

# Sistemas Externos

O A2-Gestamp-App depende de equipamentos externos para executar suas funcionalidades.

Essas integrações fazem parte do domínio da aplicação e representam a origem ou destino das informações processadas pelo sistema.

---

## Sistema de Visão (Keyence)

**IP**: 192.168.70.32

Responsável por realizar a inspeção das peças.

O sistema envia automaticamente uma nova inspeção para o aplicativo através de comunicação Non-Procedural.

Dados recebidos:

- Resultado da inspeção (GOOD / NG)
- Imagens da inspeção
- Ferramentas que julgaram a peça
- Data e hora da inspeção

Observações:

- O aplicativo nunca realiza a inspeção.
- O aplicativo apenas consome o resultado enviado pelo sistema de visão.

---

## Face ID (Hikvision)

**API**: http://192.168.70.30/ISAPI
**username**: admin
**password**: @2Vision

Responsável pela autenticação dos operadores.

O aplicativo solicita a autenticação apenas quando necessário.

Após o reconhecimento facial, o equipamento retorna os dados do operador através da API.

Dados utilizados:

- Nome do operador

Observações:

- O aplicativo não possui cadastro local de usuários.
- O equipamento Hikvision é a única fonte de autenticação.
- Cliente HTTP (enviar comando para iniciar o reconhecimento).
- Host HTTP (receber o callback da Hikvision).

---

## PLC

**IP**: 192.168.70.31

Responsável pelo controle da máquina e dos dispositivos físicos.

O aplicativo envia comandos ao PLC para controlar o fluxo da produção.

Exemplos:

- Liberação da produção
- Bloqueio da máquina
- Sinalização de estados

A definição dos endereços e bits será realizada durante a integração.