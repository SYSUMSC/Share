#include <string>
#include <iostream>
#include <map>
using namespace std;

string json;
string::iterator ptr;

struct obj
{
	map<string, obj *> child;

	string value;

	bool is_obj;

	obj() : is_obj(true) {}
	obj(const string &value) : value(value), is_obj(false) {}
};

void skip_non_blank_char()
{
	while (isspace(*ptr))
		*ptr++;
}

char peek_char()
{
	skip_non_blank_char();
	return *ptr;
}

char next_char()
{
	skip_non_blank_char();
	return *ptr++;
}

string parse_string()
{
	next_char(); // "
	char c;
	bool escape = false;
	string token;
	while (c = next_char(), true)
	{
		if (!escape)
		{
			if (c == '"')
				break;
			else if (c == '\\')
			{
				escape = true;
				continue;
			}
		}
		else
			escape = false;
		token += c;
	}
	return token;
}

obj *parse_value()
{
	if (peek_char() == '{') // an object
	{
		next_char();

        obj *res = new obj();

        while (true)
		{
            if (peek_char() != '"')
                break;
            string key = parse_string();
			next_char(); // :
			obj *val = parse_value();
			res->child[key] = val;
			if (peek_char() == '}')
				break;
			next_char(); // ,
		}

		next_char(); // }
		return res;
	}
	else
	{
		return new obj(parse_string());
	}
}

int main()
{
	int n, q;
	string line;
	cin >> n >> q;
	getline(cin, line);

	for (int i = 1; i <= n; ++i)
	{
		getline(cin, line);
		json += line;
	}

	ptr = json.begin();
	obj *root = parse_value();

	while (q--)
	{
		getline(cin, line);

		string t;
		obj *cur = root;
		bool not_found = false;
		for (int i = 0; i <= line.size(); ++i)
		{
			if (i == line.size() || line[i] == '.')
			{
				if (!cur->child.count(t))
				{
					not_found = true;
					break;
				}
				cur = cur->child[t];
				t = "";
			}
			else
			{
				t += line[i];
			}
		}

		if (not_found)
		{
			cout << "NOTEXIST" << endl;
		}
		else if (cur->is_obj)
		{
			cout << "OBJECT" << endl;
		}
		else
		{
			cout << "STRING " << cur->value << endl;
		}
	}

	return 0;
}
