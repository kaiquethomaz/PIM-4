// =====================================================================
// Comportamentos comuns dos formulários (todas as telas)
// =====================================================================
document.addEventListener("DOMContentLoaded", function () {

    // -----------------------------------------------------------------
    // 1. Evita envio duplicado (duplo clique)
    //    Sem isso, dois cliques rápidos em "Registrar" gravariam a venda 2 vezes.
    //    Depois do primeiro envio, o botão fica desabilitado e mostra
    //    "Salvando…" (ou o texto de data-texto-enviando). aria-busy avisa
    //    o leitor de tela que a ação está em andamento.
    // -----------------------------------------------------------------
    document.querySelectorAll("form").forEach(function (form) {
        if ((form.getAttribute("method") || "get").toLowerCase() !== "post") return;

        form.addEventListener("submit", function (evento) {
            if (form.dataset.enviando === "sim") {
                evento.preventDefault();   // segundo clique: ignora
                return;
            }
            form.dataset.enviando = "sim";

            form.querySelectorAll("button[type='submit']").forEach(function (botao) {
                botao.dataset.textoOriginal = botao.innerHTML;
                botao.disabled = true;
                botao.setAttribute("aria-busy", "true");
                botao.textContent = botao.dataset.textoEnviando || "Salvando…";
            });
        });
    });

    // Se o usuário voltar com o botão "Voltar" do navegador, a página pode vir
    // da memória com o botão ainda desabilitado: aqui ele é liberado de novo.
    window.addEventListener("pageshow", function () {
        document.querySelectorAll("form[data-enviando='sim']").forEach(function (form) {
            form.dataset.enviando = "";
            form.querySelectorAll("button[type='submit']").forEach(function (botao) {
                botao.disabled = false;
                botao.removeAttribute("aria-busy");
                if (botao.dataset.textoOriginal) botao.innerHTML = botao.dataset.textoOriginal;
            });
        });
    });

    // -----------------------------------------------------------------
    // 2. Máscaras: formatam o campo enquanto a pessoa digita
    //    Uso no HTML: <input data-mascara="cnpj"> / "telefone" / "preco"
    //    O servidor continua validando tudo (a máscara é só conforto).
    // -----------------------------------------------------------------
    var mascaras = {
        // 11222333000181 -> 11.222.333/0001-81
        cnpj: function (valor) {
            var d = valor.replace(/\D/g, "").slice(0, 14);
            return d.replace(/^(\d{2})(\d)/, "$1.$2")
                    .replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3")
                    .replace(/\.(\d{3})(\d)/, ".$1/$2")
                    .replace(/(\d{4})(\d)/, "$1-$2");
        },
        // 1140001000 -> (11) 4000-1000 | 11987654321 -> (11) 98765-4321
        telefone: function (valor) {
            var d = valor.replace(/\D/g, "").slice(0, 11);
            if (d.length <= 2) return d.length ? "(" + d : "";
            if (d.length <= 6) return "(" + d.slice(0, 2) + ") " + d.slice(2);
            if (d.length <= 10) return "(" + d.slice(0, 2) + ") " + d.slice(2, 6) + "-" + d.slice(6);
            return "(" + d.slice(0, 2) + ") " + d.slice(2, 7) + "-" + d.slice(7);
        },
        // Só números e UMA vírgula, com no máximo 2 casas: "28,90"
        preco: function (valor) {
            var limpo = valor.replace(/\./g, ",").replace(/[^\d,]/g, "");
            var partes = limpo.split(",");
            if (partes.length > 1) limpo = partes[0] + "," + partes.slice(1).join("").slice(0, 2);
            return limpo;
        }
    };

    document.querySelectorAll("[data-mascara]").forEach(function (campo) {
        var aplicar = mascaras[campo.dataset.mascara];
        if (!aplicar) return;

        campo.addEventListener("input", function () {
            campo.value = aplicar(campo.value);
        });
        if (campo.value) campo.value = aplicar(campo.value);   // valor que já veio do servidor
    });
});
