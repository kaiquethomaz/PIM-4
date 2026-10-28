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
