from flask import Flask, jsonify, request
import requests

app = Flask(__name__)

livros_memoria = []

@app.route('/')
def gome():
    return jsonify({'mensagem': 'API Rest do Mini-Curso está rodando'}), 200

@app.route('/api/users', methods=['GET'])
def get_users():
    response = response.get(f'https://jsonplaceholder.typicode.com/users')
    if response.status_code == 200:
        return jsonify(response.json()), 200

    return jsonify({'error': 'Erro ao acessar API externa'}), response.status_code

# @GET - Listar todos os livros
@app.route('/livros', methods=['GET'])
def listar_livros():
    return jsonify(livros_memoria), 200

# POST - Adicionar livro
@app.route('/livros', methods=['POST'])
def adicionar_livro():
    dados = request.get_json()

    if not dados.get('titulo') or not dados.get('autos'):
        return jsonify({'erro': 'Titulo e autor são obrigatorios'}), 400
    
    livro = {
        'id': len(livros_memoria) + 1,
        'titulo': dados['titulo'],
        'autor': dados['autor'],
        'ano': dados['ano'],
        'genero': dados['genero'],
    }
    livros_memoria.append(livro)
    return jsonify(livro), 201

# PUT - Atualizar livro existente
@app.route('/livros/<int:id>', methods=['PUT'])
def atualizar_livro(id):
    dados = request.get_json()
    for livro in livros_memoria:
        if livro['id'] == id:
            livro['titulo'] = dados.get('titulo', livro['titulo'])
            livro['autor'] = dados.get('autor', livro['autor'])
            livro['ano'] = dados.get('ano', livro['ano'])
            livro['genero'] = dados.get('genero', livro['genero'])

            return jsonify(livro), 200
    return jsonify({'erro': 'Livro não encontrado'}), 404
    
#DELETE -Deletar livro
@app.route('/livros/<int:id>', methods=['DELETE'])
def deletar_livro(id):
    for livro in livros_memoria:
        if livro['id'] == id:
            livros_memoria.remove(livro)
            return jsonify({'mensagem':'Livro removido com sucesso'}), 200
    return jsonify({'erro':'Livro não encontrado'}), 404

if __name__=='__main__':
    app.run(debug=True)