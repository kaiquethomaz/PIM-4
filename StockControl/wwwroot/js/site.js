// Avisos "toast" (mensagens de sucesso e erro): somem sozinhos depois de
// alguns segundos, mas o tempo pausa com o mouse em cima ou com o foco do
// teclado, e o botão × fecha na hora.
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".toast").forEach(function (toast) {
        var restante = parseInt(toast.dataset.duracao || "6000", 10);
        var inicio, timer;

        function fechar() {
            clearTimeout(timer);
            toast.classList.add("toast-saindo");
            setTimeout(function () { toast.remove(); }, 200);
        }
        function iniciar() { inicio = Date.now(); timer = setTimeout(fechar, restante); }
        function pausar() { clearTimeout(timer); restante -= Date.now() - inicio; }

        toast.querySelector(".toast-fechar").addEventListener("click", fechar);
        toast.addEventListener("mouseenter", pausar);
        toast.addEventListener("mouseleave", iniciar);
        toast.addEventListener("focusin", pausar);
        toast.addEventListener("focusout", iniciar);
        iniciar();
    });
});

// Confirmação de exclusão em janela (<dialog>).
// Links com data-confirmar abrem a janela em vez de ir para a página de confirmação.
// O formulário da janela envia um POST para o mesmo endereço do link (com o token anti-CSRF).
document.addEventListener("DOMContentLoaded", function () {
    var dialogo = document.getElementById("dialogo-confirmacao");
    if (!dialogo || typeof dialogo.showModal !== "function") return;   // navegador antigo: usa a página

    var form = document.getElementById("dialogo-form");
    var titulo = document.getElementById("dialogo-titulo");
    var mensagem = document.getElementById("dialogo-mensagem");
    var cancelar = document.getElementById("dialogo-cancelar");
    var quemAbriu = null;

    document.querySelectorAll("[data-confirmar]").forEach(function (link) {
        link.addEventListener("click", function (evento) {
            evento.preventDefault();
            quemAbriu = link;
            titulo.textContent = link.dataset.confirmarTitulo || "Confirmar exclusão";
            mensagem.textContent = link.dataset.confirmar;
            form.action = link.href;
            dialogo.showModal();
            cancelar.focus();   // foco na opção segura
        });
    });

    cancelar.addEventListener("click", function () { dialogo.close(); });

    // Clique no fundo escuro (fora da caixa) também cancela.
    dialogo.addEventListener("click", function (evento) {
        if (evento.target === dialogo) dialogo.close();
    });

    // Ao fechar (Cancelar, Esc ou fundo), o foco volta para o link que abriu a janela.
    dialogo.addEventListener("close", function () {
        if (quemAbriu) quemAbriu.focus();
    });
});

// Modo escuro: o botão da barra superior alterna o tema e guarda a escolha
// no navegador (localStorage). aria-pressed informa o estado ao leitor de tela.
document.addEventListener("DOMContentLoaded", function () {
    var botao = document.getElementById("botao-tema");
    if (!botao) return;

    function atualizarBotao() {
        var escuro = document.documentElement.getAttribute("data-tema") === "escuro";
        botao.setAttribute("aria-pressed", escuro ? "true" : "false");
    }

    botao.addEventListener("click", function () {
        var novo = document.documentElement.getAttribute("data-tema") === "escuro" ? "claro" : "escuro";
        document.documentElement.setAttribute("data-tema", novo);
        try { localStorage.setItem("stockcontrol-tema", novo); } catch (e) { }
        atualizarBotao();
    });

    atualizarBotao();
});

// Menu lateral no celular: abre e fecha como uma "gaveta".
// - aria-expanded avisa o leitor de tela se o menu está aberto;
// - a tecla Esc e o clique no fundo escuro fecham o menu;
// - ao abrir, o foco vai para o botão de fechar (navegação por teclado).
document.addEventListener("DOMContentLoaded", function () {
    var botaoAbrir = document.getElementById("botao-menu");
    var botaoFechar = document.getElementById("botao-fechar-menu");
    var fundo = document.getElementById("sidebar-fundo");
    if (!botaoAbrir || !botaoFechar || !fundo) return;

    function abrir() {
        document.body.classList.add("menu-aberto");
        botaoAbrir.setAttribute("aria-expanded", "true");
        fundo.hidden = false;
        botaoFechar.focus();
    }

    function fechar() {
        document.body.classList.remove("menu-aberto");
        botaoAbrir.setAttribute("aria-expanded", "false");
        fundo.hidden = true;
        botaoAbrir.focus();
    }

    botaoAbrir.addEventListener("click", abrir);
    botaoFechar.addEventListener("click", fechar);
    fundo.addEventListener("click", fechar);
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && document.body.classList.contains("menu-aberto")) fechar();
    });
});
